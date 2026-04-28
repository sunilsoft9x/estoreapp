using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 
using MyEstore.Models; // For RoleModel reference
using MyEstore.DTOs; // For ProductResponseDto reference
namespace MyEstore.DTOs
{
    public class AddToCartDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        [Range(1, int.MaxValue,ErrorMessage = "Product ID must be a positive integer")]
        public int ProductId { get; set;} // FKey to Product
        
        [Required(ErrorMessage = "Quantity is required")]

        [Range(1, 100,ErrorMessage = "Quantity must be a positive integer")]
        public int Quantity { get; set;} = 1; // Default quantity is 1, can be updated by the user
    }
}