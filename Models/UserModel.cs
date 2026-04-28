namespace MyEstore.Models;
using System.ComponentModel.DataAnnotations; //Validations
using System.ComponentModel.DataAnnotations.Schema;//Relationships

public class UserModel
{
    [Key] // Primary Key
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [MinLength(10)]
    [MaxLength(150)]
    //Never store plain text passwords, always store hashed passwords
    public string PasswordHash { get; set; }
    [Required]
    [MaxLength(10)]
    public string PhoneNumber { get; set; }
    [MaxLength(200)]
    public string? Address { get; set; }
    [MaxLength(30)]
    public string? City { get; set; }
    [MaxLength(30)]
    public string? State { get; set; }
    [MaxLength(30)]
    public string? Country { get; set; }
    [MaxLength(6)]
    public string? PinCode { get; set; }    

    public string? ProfileImageUrl { get; set; }
    //Role Relationship
    public int RoleId { get; set; }
    //Navigation Property
    public RoleModel? Role { get; set; }
    public CartModel? Cart { get; set; }
    //Account Status
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsDeleted { get; set; } = false;

    // Email verification
    [MaxLength(128)]
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationExpiry { get; set; }

    //Security Tracking
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LastLoginAttempt { get; set; }
    public DateTime? LastSuccessfulLogin { get; set; }
    public DateTime? LockoutEndTime { get; set; }

    //Audit Fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<OrderModel> Orders { get; set; } = new List<OrderModel>();

}