using System.ComponentModel.DataAnnotations;

namespace MyEstore.DTOs;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
