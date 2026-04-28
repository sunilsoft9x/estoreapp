using Microsoft.EntityFrameworkCore;
using MyEstore.Data;
using MyEstore.DTOs;
using MyEstore.Exceptions;
using MyEstore.Models;
using MyEstore.Services.Interfaces;

namespace MyEstore.Services.Implementations;

public class CouponService : ICouponService
{
    private readonly AppDbContext _dbContext;

    public CouponService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CouponResponseDto> CreateCouponAsync(CreateCouponDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();

        if (await _dbContext.Coupons.AnyAsync(c => c.Code == code))
            throw new ValidationException($"Coupon code '{code}' already exists.");

        var coupon = new CouponModel
        {
            Code             = code,
            Description      = dto.Description,
            DiscountType     = dto.DiscountType,
            DiscountValue    = dto.DiscountValue,
            MinOrderAmount   = dto.MinOrderAmount,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            MaxUses          = dto.MaxUses,
            MaxUsesPerUser   = dto.MaxUsesPerUser,
            ExpiresAt        = dto.ExpiresAt,
            IsActive         = true,
            CreatedAt        = DateTime.UtcNow
        };

        await _dbContext.Coupons.AddAsync(coupon);
        await _dbContext.SaveChangesAsync();

        return MapCoupon(coupon);
    }

    public async Task<IEnumerable<CouponResponseDto>> GetAllCouponsAsync()
    {
        var coupons = await _dbContext.Coupons
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return coupons.Select(MapCoupon);
    }

    public async Task<ValidateCouponResponseDto> ValidateCouponAsync(
        string code, int userId, decimal orderSubtotal)
    {
        var upper = code.Trim().ToUpperInvariant();
        var coupon = await _dbContext.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == upper);

        if (coupon is null || !coupon.IsActive)
            return Invalid("Coupon code is invalid or inactive.");

        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < DateTime.UtcNow)
            return Invalid("This coupon has expired.");

        if (coupon.MaxUses > 0 && coupon.UsedCount >= coupon.MaxUses)
            return Invalid("This coupon has reached its maximum usage limit.");

        if (orderSubtotal < coupon.MinOrderAmount)
            return Invalid($"Minimum order amount of ₹{coupon.MinOrderAmount:F2} required for this coupon.");

        // Per-user usage check
        var userUsage = await _dbContext.CouponUsages
            .CountAsync(cu => cu.CouponId == coupon.Id && cu.UserId == userId);

        if (userUsage >= coupon.MaxUsesPerUser)
            return Invalid("You have already used this coupon the maximum number of times.");

        // Calculate discount
        decimal discount;
        if (coupon.DiscountType.Equals("Percentage", StringComparison.OrdinalIgnoreCase))
        {
            discount = Math.Round(orderSubtotal * (coupon.DiscountValue / 100m), 2);
            if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
                discount = coupon.MaxDiscountAmount.Value;
        }
        else // Flat
        {
            discount = Math.Min(coupon.DiscountValue, orderSubtotal);
        }

        return new ValidateCouponResponseDto
        {
            IsValid        = true,
            Message        = $"Coupon applied! You save ₹{discount:F2}.",
            DiscountAmount = discount,
            CouponCode     = coupon.Code
        };
    }

    public async Task RecordUsageAsync(int couponId, int userId, int orderId)
    {
        await _dbContext.CouponUsages.AddAsync(new CouponUsageModel
        {
            CouponId = couponId,
            UserId   = userId,
            OrderId  = orderId,
            UsedAt   = DateTime.UtcNow
        });

        var coupon = await _dbContext.Coupons.FindAsync(couponId);
        if (coupon is not null)
            coupon.UsedCount++;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeactivateCouponAsync(int couponId)
    {
        var coupon = await _dbContext.Coupons.FindAsync(couponId)
            ?? throw new NotFoundException("Coupon not found.");

        coupon.IsActive = false;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static ValidateCouponResponseDto Invalid(string message)
        => new() { IsValid = false, Message = message, DiscountAmount = 0 };

    private static CouponResponseDto MapCoupon(CouponModel c) => new()
    {
        Id               = c.Id,
        Code             = c.Code,
        Description      = c.Description,
        DiscountType     = c.DiscountType,
        DiscountValue    = c.DiscountValue,
        MinOrderAmount   = c.MinOrderAmount,
        MaxDiscountAmount = c.MaxDiscountAmount,
        MaxUses          = c.MaxUses,
        UsedCount        = c.UsedCount,
        MaxUsesPerUser   = c.MaxUsesPerUser,
        IsActive         = c.IsActive,
        ExpiresAt        = c.ExpiresAt,
        CreatedAt        = c.CreatedAt
    };
}
