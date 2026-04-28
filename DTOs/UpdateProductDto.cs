namespace MyEstore.DTOs;

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public decimal? Price { get; set; }
    public decimal? Discount { get; set; }
    public int? StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsActive { get; set; }
}
