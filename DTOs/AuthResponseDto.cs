namespace MyEstore.DTOs;

public class AuthResponseDto
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public bool OtpRequired { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserResponseDto? User { get; set; }
}
