using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyEstore.Data;
using MyEstore.Models;
using MyEstore.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyEstore.Services.Implementations;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;

    // Refresh tokens are valid for 7 days by default; configurable via Jwt:RefreshTokenExpiryDays
    private int RefreshTokenExpiryDays =>
        int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out var d) ? d : 7;

    public TokenService(IConfiguration configuration, AppDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

    // ── Access token ──────────────────────────────────────────────────────────
    public string GenerateAccessToken(UserModel user, string roleName)
    {
        var jwtKey   = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key missing.");
        var issuer   = _configuration["Jwt:Issuer"]   ?? "MyEstore";
        var audience = _configuration["Jwt:Audience"] ?? "MyEstoreUsers";
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var p) ? p : 30;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Role,           roleName),
            new("firstName",               user.FirstName),
            new("lastName",                user.LastName)
        };

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer, audience, claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── Refresh token helpers ─────────────────────────────────────────────────
    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes);
    }

    public async Task StoreRefreshTokenAsync(int userId, string rawToken, string? deviceInfo = null)
    {
        // Prune expired/revoked tokens for this user to keep the table lean
        var stale = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && (rt.IsRevoked || rt.ExpiresAt < DateTime.UtcNow))
            .ToListAsync();
        _dbContext.RefreshTokens.RemoveRange(stale);

        await _dbContext.RefreshTokens.AddAsync(new RefreshTokenModel
        {
            UserId     = userId,
            TokenHash  = HashToken(rawToken),
            ExpiresAt  = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
            DeviceInfo = deviceInfo,
            CreatedAt  = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> ValidateRefreshTokenAsync(int userId, string rawToken)
    {
        var hash = HashToken(rawToken);
        return await _dbContext.RefreshTokens.AnyAsync(rt =>
            rt.UserId    == userId &&
            rt.TokenHash == hash &&
            !rt.IsRevoked &&
            rt.ExpiresAt  > DateTime.UtcNow);
    }

    public async Task<int?> GetUserIdByRefreshTokenAsync(string rawToken)
    {
        var hash = HashToken(rawToken);
        var record = await _dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == hash &&
                !rt.IsRevoked &&
                rt.ExpiresAt > DateTime.UtcNow);
        return record?.UserId;
    }

    public async Task RevokeRefreshTokenAsync(int userId, string rawToken)
    {
        var hash   = HashToken(rawToken);
        var record = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.TokenHash == hash);
        if (record is not null)
        {
            record.IsRevoked = true;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RevokeAllRefreshTokensAsync(int userId)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();
        foreach (var t in tokens) t.IsRevoked = true;
        await _dbContext.SaveChangesAsync();
    }
}
