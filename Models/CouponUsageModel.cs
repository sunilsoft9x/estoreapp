using System.ComponentModel.DataAnnotations;

namespace MyEstore.Models;

public class CouponUsageModel
{
    [Key]
    public int Id { get; set; }

    public int CouponId { get; set; }
    public CouponModel? Coupon { get; set; }

    public int UserId { get; set; }

    public int OrderId { get; set; }

    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
}
