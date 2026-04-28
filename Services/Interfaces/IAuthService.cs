using MyEstore.Models;

namespace MyEstore.Services.Interfaces;

public interface IAuthService
{
    // Generates a signed JWT access token for the specified user and role
    Task<string> GenerateJwtToken(UserModel user, string roleName);

    // Derives a salted PBKDF2 hash of the given plain-text password
    Task<string> HashPassword(string password);

    // Verifies a plain-text password against a stored salt:hash pair
    Task<bool> VerifyPassword(string enteredPassword, string storedHash);
}
