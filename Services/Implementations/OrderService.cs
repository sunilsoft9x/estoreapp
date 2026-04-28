using Microsoft.EntityFrameworkCore;
using MyEstore.Data;
using MyEstore.DTOs;
using MyEstore.Exceptions;
using MyEstore.Models;
using MyEstore.Services.Interfaces;

namespace MyEstore.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly ICouponService _couponService;

    public OrderService(AppDbContext dbContext, ICouponService couponService)
    {
        _dbContext = dbContext;
        _couponService = couponService;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(int userId, CreateOrderDto dto)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.CartItems!)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive)
            ?? throw new ValidationException("Cart not found.");

        if (cart.CartItems is null || cart.CartItems.Count == 0)
        {
            throw new ValidationException("Cannot create order from an empty cart.");
        }

        await using var tx = await _dbContext.Database.BeginTransactionAsync();

        foreach (var item in cart.CartItems)
        {
            if (item.Product is null || item.Product.IsDeleted || !item.Product.IsActive)
            {
                throw new ValidationException($"Product {item.ProductId} is not available.");
            }

            if (item.Quantity > item.Product.StockQuantity)
            {
                throw new ValidationException($"Insufficient stock for product {item.Product.Name}.");
            }
        }

        var orderItems = cart.CartItems.Select(ci => new OrderItemModel
        {
            ProductId = ci.ProductId,
            ProductName = ci.Product!.Name,
            ProductImageUrl = ci.Product.ImageUrl,
            Quantity = ci.Quantity,
            UnitPrice = ci.UnitPrice,
            TotalPrice = ci.Quantity * ci.UnitPrice,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        var subtotal = orderItems.Sum(i => i.TotalPrice);
        var tax = Math.Round(subtotal * 0.18m, 2);
        var shipping = subtotal > 999 ? 0 : 50;

        // Apply coupon if provided
        decimal discountAmount = 0;
        int? appliedCouponId = null;
        string? appliedCouponCode = null;

        if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        {
            var couponResult = await _couponService.ValidateCouponAsync(dto.CouponCode, userId, subtotal);
            if (!couponResult.IsValid)
                throw new ValidationException(couponResult.Message ?? "Invalid coupon code.");

            discountAmount = couponResult.DiscountAmount;
            appliedCouponCode = couponResult.CouponCode;

            // Resolve coupon entity for FK
            var couponEntity = await _dbContext.Coupons
                .FirstOrDefaultAsync(c => c.Code == appliedCouponCode);
            appliedCouponId = couponEntity?.Id;
        }

        var order = new OrderModel
        {
            UserId = userId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            TotalAmount = subtotal + tax + shipping - discountAmount,
            DiscountAmount = discountAmount > 0 ? discountAmount : null,
            TaxAmount = tax,
            ShippingCost = shipping,
            Status = "Pending",
            PaymentStatus = "Pending",
            ShippingAddress = dto.ShippingAddress,
            ShippingCity = dto.City,
            ShippingState = dto.State,
            ShippingPinCode = dto.PINCode,
            ShippingCountry = dto.Country,
            CouponId = appliedCouponId,
            AppliedCouponCode = appliedCouponCode,
            CreatedAt = DateTime.UtcNow,
            OrderItems = orderItems
        };

        await _dbContext.Orders.AddAsync(order);

        foreach (var item in cart.CartItems)
        {
            item.Product!.StockQuantity -= item.Quantity;
        }

        _dbContext.CartItems.RemoveRange(cart.CartItems);
        await _dbContext.SaveChangesAsync();

        // Record coupon usage after order is persisted
        if (appliedCouponId.HasValue)
            await _couponService.RecordUsageAsync(appliedCouponId.Value, userId, order.Id);

        await tx.CommitAsync();

        return MapOrder(order);
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(int orderId, int requestingUserId, bool isAdmin)
    {
        var order = await _dbContext.Orders.AsNoTracking()
            .Include(o => o.OrderItems!)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException("Order not found.");

        if (!isAdmin && order.UserId != requestingUserId)
            throw new UnauthorizedException("Access denied.");

        return MapOrder(order);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(int userId)
    {
        var orders = await _dbContext.Orders.AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems!)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapOrder).ToList();
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var orders = await _dbContext.Orders.AsNoTracking()
            .Include(o => o.OrderItems!)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return orders.Select(MapOrder).ToList();
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException("Order not found.");

        var next = status.Trim();
        var allowed = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Pending"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Confirmed", "Cancelled" },
            ["Confirmed"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Shipped", "Cancelled" },
            ["Shipped"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Delivered" },
            ["Delivered"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            ["Cancelled"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        };

        if (!allowed.TryGetValue(order.Status, out var allowedNext) || !allowedNext.Contains(next))
        {
            throw new ValidationException($"Invalid order status transition: {order.Status} -> {next}");
        }

        order.Status = next;
        order.UpdatedAt = DateTime.UtcNow;

        if (next.Equals("Shipped", StringComparison.OrdinalIgnoreCase))
        {
            order.ShippedAt = DateTime.UtcNow;
        }

        if (next.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
        {
            order.DeliveredAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelOrderAsync(int orderId, int requestingUserId, bool isAdmin)
    {
        var order = await _dbContext.Orders.Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException("Order not found.");

        if (!isAdmin && order.UserId != requestingUserId)
            throw new UnauthorizedException("Access denied.");

        if (!order.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)
            && !order.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Only pending or confirmed orders can be cancelled.");
        }

        order.Status = "Cancelled";
        order.UpdatedAt = DateTime.UtcNow;

        foreach (var item in order.OrderItems ?? new List<OrderItemModel>())
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
            if (product is not null)
            {
                product.StockQuantity += item.Quantity;
            }
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static OrderResponseDto MapOrder(OrderModel order)
    {
        return new OrderResponseDto
        {
            OrderNumber = order.OrderNumber,
            Items = (order.OrderItems ?? new List<OrderItemModel>()).Select(oi => new OrderItemResponseDto
            {
                ProductName = oi.ProductName,
                ProductImageUrl = oi.ProductImageUrl,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList(),
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            TaxAmount = order.TaxAmount,
            ShippingCost = order.ShippingCost,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            ShippingAddress = order.ShippingAddress,
            ShippingCity = order.ShippingCity,
            ShippingState = order.ShippingState,
            ShippingPinCode = order.ShippingPinCode,
            ShippingCountry = order.ShippingCountry,
            CreatedAt = order.CreatedAt,
            DeliveredAt = order.DeliveredAt
        };
    }
}
