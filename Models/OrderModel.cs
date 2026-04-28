using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyEstore.Models;

namespace MyEstore.Models
{
    public class OrderModel
    {
        [Key]
        public int Id{ get; set; } // Primary Key
        [Required]
        public int UserId { get; set; } // FKey to User
        // Navigation Property
        public UserModel? User { get; set; }
        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = String.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ShippingCost { get; set; }
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // e.g., Pending, Processing, Shipped, Delivered, Cancelled
        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending"; // e.g., Pending, Paid,Failed, Refunded
        //shipping details
        [MaxLength(200)]
        public string? ShippingAddress { get; set; }
        [MaxLength(30)]
        public string? ShippingCity { get; set; }
        [MaxLength(20)]
        public string? ShippingState { get; set; }
        [MaxLength(6)]
        public string? ShippingPinCode { get; set; }
        public string? ShippingCountry { get; set; }
        //Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        //Order Items Relationship
        public List<OrderItemModel>? OrderItems { get; set; }

        // Coupon applied to this order
        public int? CouponId { get; set; }
        public CouponModel? Coupon { get; set; }
        [MaxLength(30)]
        public string? AppliedCouponCode { get; set; }
    }
}