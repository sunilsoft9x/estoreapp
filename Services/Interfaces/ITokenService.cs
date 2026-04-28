using MyEstore.Models;

namespace MyEstore.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(UserModel user, string roleName);
    string GenerateRefreshToken();
    Task StoreRefreshTokenAsync(int userId, string rawToken, string? deviceInfo = null);
    Task<bool> ValidateRefreshTokenAsync(int userId, string rawToken);
    Task<int?> GetUserIdByRefreshTokenAsync(string rawToken);
    Task RevokeRefreshTokenAsync(int userId, string rawToken);
    Task RevokeAllRefreshTokensAsync(int userId);
}
