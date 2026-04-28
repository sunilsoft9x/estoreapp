using System.ComponentModel.DataAnnotations;

namespace MyEstore.DTOs;

public class PaymentRequestDto
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(100)]
    public string PaymentGateway { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;
}
