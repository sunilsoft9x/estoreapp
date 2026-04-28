using MyEstore.DTOs;

namespace MyEstore.Services.Interfaces;

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshAccessTokenAsync(int userId, string rawRefreshToken);
    Task<bool> SendOtpAsync(string email);
    Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto dto);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ResendVerificationEmailAsync(string email);
    Task<UserResponseDto> GetUserByIdAsync(int userId);
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int page = 1, int pageSize = 20);
    Task<bool> UpdateUserAsync(int userId, UpdateUserDto dto);
    Task<bool> DeleteUserAsync(int userId);
}
