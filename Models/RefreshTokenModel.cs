using System.ComponentModel.DataAnnotations;

namespace MyEstore.Models;

public class RefreshTokenModel
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public UserModel? User { get; set; }

    /// <summary>SHA-256 hash of the raw refresh token — raw token is never persisted.</summary>
    [Required]
    [MaxLength(256)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(300)]
    public string? DeviceInfo { get; set; }
}
