using MyEstore.DTOs;

namespace MyEstore.Services.Interfaces;

public interface ICouponService
{
    Task<CouponResponseDto> CreateCouponAsync(CreateCouponDto dto);
    Task<IEnumerable<CouponResponseDto>> GetAllCouponsAsync();
    Task<ValidateCouponResponseDto> ValidateCouponAsync(string code, int userId, decimal orderSubtotal);
    Task RecordUsageAsync(int couponId, int userId, int orderId);
    Task<bool> DeactivateCouponAsync(int couponId);
}
