using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyEstore.Models;
namespace MyEstore.Models
{
    public class OrderItemModel
    {
        [Key]
        public int Id{get; set;} // Primary Key
        //Order Relationship
        public int OrderId { get; set; } // FKey to Order
        // Navigation Property
        public OrderModel? Order { get; set; }
        //Product Relationship
        public int ProductId { get; set; } // FKey to Product
        // Navigation Property
        public ProductModel? Product { get; set; }
        [Required]
        [MaxLength(150)]
        public string ProductName { get; set; } = String.Empty; // Store product name at the time of order
        public string? ProductImageUrl { get; set; } // Store product image URL at the time of order
        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; } // Stored Value not Computed here
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}