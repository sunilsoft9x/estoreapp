//This is for user registration, capturing necessary details for account creation and validation.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyEstore.Models; // For RoleModel reference
namespace MyEstore.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName{ get; set;} = string.Empty;
        [Required]
        [MaxLength(50)]
        public string LastName{ get; set;} = string.Empty;
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100)]
        public string Email{ get; set;} = string.Empty;

        //Password must be strong and complex has special charcters, numbers and letters
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [MaxLength(25, ErrorMessage = "Password cannot exceed 25 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string Password{ get; set;} = string.Empty;
        //Confirm password field to ensure user input accuracy, not stored in database
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [NotMapped] // This field is not mapped to the database
        public string ConfirmPassword { get; set;} = string.Empty;
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set;} = string.Empty;

    }
}
/*
{
  "firstName": "Sunny",
  "lastName": "Dhawan",
  "email": "sunny@gmail.com",
  "password": "12345@1Aa6",
  "confirmPassword": "12345@1Aa6",
  "phoneNumber": "9876543210"
}
*/