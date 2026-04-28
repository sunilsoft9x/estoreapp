using MyEstore.Models;
using System.ComponentModel.DataAnnotations; //Validations
using System.ComponentModel.DataAnnotations.Schema;//Relationships
namespace MyEstore.Models
{
    public class OtpVerificationModel
    {
        [Key]
        public int Id { get; set; }
        //User Relationship
        [Required]
        [MaxLength(150)]
        public string Email {get;set;} = string.Empty; // Store email for which OTP is generated
        [Required]
        [MaxLength(64)]
        public string OtpCode{get;set;} = string.Empty; // Store the hashed OTP code (SHA-256 hex = 64 chars)
        public string Purpose { get; set; } = string.Empty; // e.g., "EmailVerification", "PasswordReset"
        public DateTime ExpiryTime { get; set; } // OTP Expiry Time
        public bool IsUsed { get; set; } = false; // Track if OTP has been used
        public int AttemptCount { get; set; } = 0; // Track number of verification attempts
        public int MaxAttempts { get; set; } = 3; // Maximum allowed attempts
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VerifiedAt { get; set; }
    }

}