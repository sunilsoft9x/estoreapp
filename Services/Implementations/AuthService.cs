// JWT Namespaces
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
// Cryptography Namespaces — PBKDF2 password hashing (framework built-in, no extra package)
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
// Application Namespaces
using MyEstore.Models;
using MyEstore.Services.Interfaces;

namespace MyEstore.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // JWT Generation
    // ─────────────────────────────────────────────────────────────────────────
    public Task<string> GenerateJwtToken(UserModel user, string roleName)
    {
        var claims = new List<Claim>
        {
            new("id",                user.Id.ToString()),
            new(ClaimTypes.Email,    user.Email),
            new(ClaimTypes.Role,     roleName),
            new("firstName",         user.FirstName),
            new("lastName",          user.LastName)
        };

        var secretKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT secret key (Jwt:Key) is missing from configuration.");

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var parsed) ? parsed : 30;

        var token = new JwtSecurityToken(
            issuer:             _configuration["Jwt:Issuer"],
            audience:           _configuration["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Password Hashing — PBKDF2 with HMAC-SHA256
    // Stores <base64-salt>:<base64-hash> so the salt travels with the hash.
    // ─────────────────────────────────────────────────────────────────────────
    public Task<string> HashPassword(string password)
    {
        // Generate a cryptographically secure 16-byte (128-bit) random salt
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        // Derive a 256-bit (32-byte) key; 600,000 iterations per OWASP 2023 recommendation for PBKDF2-HMAC-SHA256
        string hash = Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password:       password,
                salt:           salt,
                prf:            KeyDerivationPrf.HMACSHA256,
                iterationCount: 600_000,
                numBytesRequested: 32
            )
        );

        // Encode salt alongside the hash so we can re-derive during verification
        return Task.FromResult($"{Convert.ToBase64String(salt)}:{hash}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Password Verification
    // Re-derives the hash using the stored salt and compares in constant time
    // to prevent timing-based side-channel attacks.
    // ─────────────────────────────────────────────────────────────────────────
    public Task<bool> VerifyPassword(string enteredPassword, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2)
        {
            return Task.FromResult(false); // Malformed stored value
        }

        byte[] salt             = Convert.FromBase64String(parts[0]);
        byte[] storedHashBytes  = Convert.FromBase64String(parts[1]);

        byte[] enteredHashBytes = KeyDerivation.Pbkdf2(
            password:       enteredPassword,
            salt:           salt,
            prf:            KeyDerivationPrf.HMACSHA256,
            iterationCount: 600_000,
            numBytesRequested: 32
        );

        // CryptographicOperations.FixedTimeEquals prevents timing attacks\
        // by ensuring the comparison takes the same amount of time regardless of how many bytes match.
        return Task.FromResult(CryptographicOperations.FixedTimeEquals(storedHashBytes, enteredHashBytes));
    }
}
