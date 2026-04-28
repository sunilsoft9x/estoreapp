namespace MyEstore.DTOs
{
    public class OrderItemResponseDto
    {
        public string ProductName { get; set;} = string.Empty;
        public string? ProductImageUrl { get; set;}
        public int Quantity { get; set;}
        public decimal UnitPrice { get; set;}
        public decimal TotalPrice { get; set;}
    }
}