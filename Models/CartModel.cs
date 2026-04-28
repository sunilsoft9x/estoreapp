using MyEstore.Models;
using System.ComponentModel.DataAnnotations; //Validations
using System.ComponentModel.DataAnnotations.Schema;//Relationships

public class CartModel
{
    [Key] // Primary Key
    public int Id{ get; set; }
    //User Relationship
    public int UserId { get; set; } // FKey
    //Navigation Property
    public UserModel? User { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    //Relationships
    public ICollection<CartItemModel> CartItems { get; set; } 
}