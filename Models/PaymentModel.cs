using MyEstore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyEstore.Models
{
    public class PaymentModel
    {
        [Key]
        public int Id { get; set; } // Primary Key
        //Order Relationship
        //OrderID - FKey to Order && Internal Comm
        //OrderNumber - External Comm
        public int OrderId { get; set; } // FKey to Order
        public OrderModel? Order { get; set; } // Navigation Property
        [Required]
        [MaxLength(100)]
        public string TransactionId { get; set; } = String.Empty; // Unique Transaction ID from Payment Gateway
        [Required]
        [MaxLength(100)]
        public string? PaymentGateway { get; set; }
        [Required]
        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // e.g., Credit Card,UPI, PayPal, etc. 
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? RefundAmount { get; set; }
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // e.g., Pending, Completed, Failed, Refunded
        public string? GatewayResponse { get; set; } // Store JSON Response
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}