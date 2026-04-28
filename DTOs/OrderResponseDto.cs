using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyEstore.Models; // For RoleModel reference
namespace MyEstore.DTOs
{
    public class OrderResponseDto
    {
        public string OrderNumber { get; set;} = string.Empty;
        // Should not be OderID as its internal & PrimaryKey 
        //should not be revealed
        public List<OrderItemResponseDto> Items { get; set;} = new List<OrderItemResponseDto>();
        public decimal TotalAmount { get; set;}
        public decimal? DiscountAmount { get; set;}
        public decimal TaxAmount { get; set;} =0;
        public decimal? ShippingCost { get; set;}
        public string Status { get; set;} = string.Empty;
        public string PaymentStatus { get; set;} = string.Empty;
        //shipping details
        public string? ShippingAddress { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingState { get; set; }
        public string? ShippingPinCode { get; set; }
        public string? ShippingCountry { get; set; }
        public DateTime CreatedAt { get; set;}
        public DateTime? DeliveredAt { get; set;}
    }
}