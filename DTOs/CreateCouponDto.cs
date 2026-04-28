using System.ComponentModel.DataAnnotations;

namespace MyEstore.DTOs;

public class CreateCouponDto
{
    [Required]
    [MaxLength(30)]
    [RegularExpression(@"^[A-Z0-9_\-]+$", ErrorMessage = "Code must be uppercase letters, digits, underscores or hyphens.")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    [Required]
    [RegularExpression("^(Percentage|Flat)$", ErrorMessage = "DiscountType must be 'Percentage' or 'Flat'.")]
    public string DiscountType { get; set; } = "Percentage";

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "DiscountValue must be greater than 0.")]
    public decimal DiscountValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinOrderAmount { get; set; } = 0;

    [Range(0.01, double.MaxValue)]
    public decimal? MaxDiscountAmount { get; set; }

    [Range(0, int.MaxValue)]
    public int MaxUses { get; set; } = 0;

    [Range(1, int.MaxValue)]
    public int MaxUsesPerUser { get; set; } = 1;

    public DateTime? ExpiresAt { get; set; }
}
