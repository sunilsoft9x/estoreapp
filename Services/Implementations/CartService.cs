using Microsoft.EntityFrameworkCore;
using MyEstore.Data;
using MyEstore.DTOs;
using MyEstore.Exceptions;
using MyEstore.Models;
using MyEstore.Services.Interfaces;

namespace MyEstore.Services.Implementations;

public class CartService : ICartService
{
    private readonly AppDbContext _dbContext;

    public CartService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CartResponseDto> GetCartAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return await BuildCartResponseAsync(cart.Id);
    }

    public async Task<CartResponseDto> AddToCartAsync(int userId, AddToCartDto dto)
    {
        if (dto.Quantity <= 0)
        {
            throw new ValidationException("Quantity must be greater than 0.");
        }

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId && !p.IsDeleted && p.IsActive)
            ?? throw new NotFoundException("Product not found.");

        if (product.StockQuantity < dto.Quantity)
        {
            throw new ValidationException("Insufficient stock.");
        }

        var cart = await GetOrCreateCartAsync(userId);

        var cartItem = await _dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == dto.ProductId);

        if (cartItem is null)
        {
            cartItem = new CartItemModel
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = product.FinalPrice ?? product.Price,
                CreatedAt = DateTime.UtcNow
            };
            await _dbContext.CartItems.AddAsync(cartItem);
        }
        else
        {
            var newQty = cartItem.Quantity + dto.Quantity;
            if (newQty > product.StockQuantity)
            {
                throw new ValidationException("Insufficient stock for requested quantity.");
            }

            cartItem.Quantity = newQty;
            cartItem.UnitPrice = product.FinalPrice ?? product.Price;
            cartItem.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return await BuildCartResponseAsync(cart.Id);
    }

    public async Task<CartResponseDto> UpdateCartItemAsync(int userId, int productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ValidationException("Quantity must be greater than 0.");
        }

        var cart = await GetOrCreateCartAsync(userId);

        var cartItem = await _dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId)
            ?? throw new NotFoundException("Cart item not found.");

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted && p.IsActive)
            ?? throw new NotFoundException("Product not found.");

        if (quantity > product.StockQuantity)
        {
            throw new ValidationException("Insufficient stock.");
        }

        cartItem.Quantity = quantity;
        cartItem.UnitPrice = product.FinalPrice ?? product.Price;
        cartItem.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return await BuildCartResponseAsync(cart.Id);
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int productId)
    {
        var cart = await GetOrCreateCartAsync(userId);

        var item = await _dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId)
            ?? throw new NotFoundException("Cart item not found.");

        _dbContext.CartItems.Remove(item);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var items = await _dbContext.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync();

        _dbContext.CartItems.RemoveRange(items);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private async Task<CartModel> GetOrCreateCartAsync(int userId)
    {
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId && !u.IsDeleted);
        if (!userExists)
        {
            throw new NotFoundException("User not found.");
        }

        var cart = await _dbContext.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);
        if (cart is not null)
        {
            return cart;
        }

        cart = new CartModel
        {
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Carts.AddAsync(cart);
        await _dbContext.SaveChangesAsync();
        return cart;
    }

    private async Task<CartResponseDto> BuildCartResponseAsync(int cartId)
    {
        var cart = await _dbContext.Carts.AsNoTracking()
            .Include(c => c.CartItems!)
            .ThenInclude(i => i.Product)
            .FirstAsync(c => c.Id == cartId);

        var items = (cart.CartItems ?? new List<CartItemModel>())
            .Select(ci => new CartItemResponseDto
            {
                ProductId = ci.ProductId,
                ProductName = ci.Product?.Name ?? string.Empty,
                ImageUrl = ci.Product?.ImageUrl,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                TotalPrice = ci.Quantity * ci.UnitPrice
            }).ToList();

        return new CartResponseDto
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            Items = items,
            GrandTotal = items.Sum(i => i.TotalPrice)
        };
    }
}
