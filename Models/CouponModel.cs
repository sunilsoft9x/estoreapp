using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyEstore.Models;

public class CouponModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>"Percentage" or "Flat"</summary>
    [Required]
    [MaxLength(20)]
    public string DiscountType { get; set; } = "Percentage";

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }

    /// <summary>Minimum order subtotal for the coupon to apply.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal MinOrderAmount { get; set; } = 0;

    /// <summary>Cap on the rupee value of discount (useful for percentage coupons).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>0 = unlimited global uses.</summary>
    public int MaxUses { get; set; } = 0;

    public int UsedCount { get; set; } = 0;

    /// <summary>How many times one user may use this coupon.</summary>
    public int MaxUsesPerUser { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CouponUsageModel> Usages { get; set; } = new List<CouponUsageModel>();
}
