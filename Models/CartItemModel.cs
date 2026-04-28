using MyEstore.Models;
using System.ComponentModel.DataAnnotations; //Validations
using System.ComponentModel.DataAnnotations.Schema;//Relationships

public class CartItemModel
{
    [Key]
    public int Id { get; set; }
    //Cart Relationship
    public int CartId { get; set; } // FKey
    //Navigation Property
    public CartModel? Cart { get; set; }
    //Product Relationship
    public int ProductId { get; set; } // FKey
    //Navigation Property
    public ProductModel? Product { get; set; }
    [Required]
    [Range(1, 1000)]
    public int Quantity { get; set; }
    [Range(0.10, 1000000.00)]
    public decimal UnitPrice { get; set; }

    [NotMapped] // This property is calculated on the fly and not stored in the database
    public decimal TotalPrice => Quantity * UnitPrice;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

}