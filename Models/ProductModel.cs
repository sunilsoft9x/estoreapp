using MyEstore.Models;
using System.ComponentModel.DataAnnotations; //Validations
using System.ComponentModel.DataAnnotations.Schema;//Relationships

public class ProductModel
{
    [Key] // Primary Key
    public int Id { get; set; }
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(300)]
    public string? Description { get; set; }
    [Required]
    public int CategoryId { get; set; } // FKey to Category
    // Navigation Property
    public CategoryModel? Category { get; set; }
    //Product Pricing
    [Range(0.10, 1000000.00)]
    [Required]
    public decimal Price { get; set; }
    [Range(0.01,100.00)]
    public decimal? Discount { get; set; }
    public decimal? FinalPrice { 
        get {
            if (Discount.HasValue)
            {
                return Price - (Price * (Discount.Value / 100));
            }
            return Price;
        }
    }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool isInStock => StockQuantity > 0;
    //Category Relationship - Pending
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    //Soft Deletion allows us to keep the product in the database for historical and analytical purposes, while marking it as inactive for users.
    public bool IsFeatured { get; set; } = false;
}