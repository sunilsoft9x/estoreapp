using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyEstore.Models; // For RoleModel reference
namespace MyEstore.DTOs
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(120, ErrorMessage = "Product name cannot exceed 120 characters")]
        public string Name { get; set;} = string.Empty;
        public string? Description { get; set;}
        [Required(ErrorMessage = "Price is required")]
        [Range(0.10, 1000000.00, ErrorMessage = "Price must be between 0.10 and 1,000,000.00")]
        public decimal Price { get; set;}
        //CategoryId is required to establish the relationship with CategoryModel, ensuring that every product is associated with a valid category.
        
        [Required(ErrorMessage = "Category ID is required")]
        [Range(1,100, ErrorMessage = "Category ID must be between 1 and 100")]
        public int CategoryId { get; set;}
        
        [Range(0.10, 1000000.00, ErrorMessage = "Price must be between 0.10 and 1,000,000.00")]

        public decimal? DiscountedPrice { get; set;} // Optional field for discounted price, allowing flexibility in pricing strategies.
        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, 10000, ErrorMessage = "Stock quantity must be between 0 and 10,000")]
        public int StockQuantity { get; set;} // Required field to manage inventory and ensure accurate stock levels.
        public List<string>? ImageUrls { get; set;} // Optional field for product image URL, enhancing the visual appeal of product listings.
        public bool IsFeatured { get; set;} = false; // Optional field to mark a product as featured, allowing for promotional highlights on the storefront.
        [Required(ErrorMessage = "Brand is required")   ]
        public string Brand { get; set;} = string.Empty; // Optional field for product brand, providing additional information for customers and enabling brand-based filtering in the storefront.
        [Required(ErrorMessage = "SKU is required")]
        [MaxLength(50)]
        public string SKU { get; set;} = String.Empty; // Stock Keeping Unit, auto-generated for unique product identification and inventory management.
        //Sepcifications
        public Dictionary<string,string>? Specifications {get; set;} // Optional field for product specifications, allowing for detailed product information and enhanced searchability based on specific attributes.
        public bool IsActive { get; set;} = true; // Optional field to indicate if the product is active, enabling soft deletion and better inventory management without permanently removing products from the database.
    }
}
/*
{
  "name": "iPhone 15",
  "description": "Latest Apple smartphone",
  "brand": "Apple",
  "sku": "APL-IP15-128",
  "price": 80000,
  "discountPrice": 75000,
  "stockQuantity": 50,
  "categoryId": 1,
  "imageUrls": [
    "img1.jpg",
    "img2.jpg"
  ],
  "specifications": {
    "RAM": "8GB",
    "Storage": "128GB",
    "Processor": "A16"
  },
  "isFeatured": true
}
*/