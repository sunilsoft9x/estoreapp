using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyEstore.Models; // For RoleModel reference
namespace MyEstore.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100)]
        public string Email{ get; set;} = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        
        public string Password{ get; set;} = string.Empty;
        public string? OTP { get; set;} // Optional field for OTP during login
        public bool IsTwoFactorEnabled { get; set;} = false; // Indicates if 2FA is enabled for the user
        public bool RememberMe { get; set;} = false; // Option for persistent login
        //Security Tracking fields
        public string? DeviceInfo { get; set;} // Optional field to capture device information for security purposes
        public string? IPAddress { get; set;} // Optional field to capture IP address for security purposes
    }
}