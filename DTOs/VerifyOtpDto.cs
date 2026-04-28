using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyEstore.Models; // For RoleModel reference
namespace MyEstore.DTOs
{
    public class VerifyOtpDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set;} = string.Empty;
        [Required(ErrorMessage = "OTP is required")]
        [MinLength(6, ErrorMessage = "OTP must be 6 characters long")]
        [MaxLength(6, ErrorMessage = "OTP must be 6 characters long")]
        public string Otp { get; set;} = string.Empty;
        public string Purpose { get; set;} = string.Empty; // To specify the purpose of OTP verification (e.g., "Login", "PasswordReset")
         public string? DeviceInfo { get; set;} // Optional field to capture device information for security purposes
        public string? IPAddress { get; set;} // Optional field to capture IP address for security purposes
    }
}