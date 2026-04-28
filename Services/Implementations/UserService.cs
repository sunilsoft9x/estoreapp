using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MyEstore.Data;
using MyEstore.DTOs;
using MyEstore.Exceptions;
using MyEstore.Models;
using MyEstore.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace MyEstore.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;

    public UserService(
        AppDbContext dbContext,
        IAuthService authService,
        IEmailService emailService,
        ITokenService tokenService,
        IMapper mapper,
        ILogger<UserService> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _authService = authService;
        _emailService = emailService;
        _tokenService = tokenService;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var exists = await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == email && !u.IsDeleted);
        if (exists)
        {
            throw new ValidationException("Email is already registered.");
        }

        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
        if (role is null)
        {
            role = new RoleModel
            {
                Name = "Customer",
                Description = "Default customer role",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _dbContext.Roles.AddAsync(role);
            await _dbContext.SaveChangesAsync();
        }

        var user = new UserModel
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = await _authService.HashPassword(dto.Password),
            RoleId = role.Id,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Generate email verification token and send (fire-and-forget; don't block registration)
        var rawVerifyToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        user.EmailVerificationToken = rawVerifyToken;
        user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);
        await _dbContext.SaveChangesAsync();

        var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5000";
        var verifyLink = $"{baseUrl}/api/auth/verify-email?token={Uri.EscapeDataString(rawVerifyToken)}";
        _ = _emailService.SendVerificationEmailAsync(email, user.FirstName, verifyLink)
              .ContinueWith(t => _logger.LogError(t.Exception, "Failed to send verification email to {Email}", email),
                            System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

        // Issue tokens
        var rawRefresh = _tokenService.GenerateRefreshToken();
        await _tokenService.StoreRefreshTokenAsync(user.Id, rawRefresh);
        var token = await _authService.GenerateJwtToken(user, role.Name);
        var userResponse = _mapper.Map<UserResponseDto>(user);
        userResponse.RoleName = role.Name;

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = rawRefresh,
            OtpRequired = false,
            Message = "Registration successful. Please check your email to verify your account.",
            User = userResponse
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email && !u.IsDeleted);

        _logger.LogInformation("Login attempt for {Email}", email);

        // Check lockout before doing any work (prevents timing oracle on locked accounts)
        if (user is not null && user.LockoutEndTime.HasValue && user.LockoutEndTime > DateTime.UtcNow)
        {
            _logger.LogWarning("Locked account login attempt for {Email}", email);
            throw new UnauthorizedException("Account is locked. Try again later.");
        }

        if (user is null || !await _authService.VerifyPassword(dto.Password, user.PasswordHash))
        {
            if (user is not null)
            {
                user.FailedLoginAttempts++;
                user.LastLoginAttempt = DateTime.UtcNow;

                // Lock account for 15 minutes after 5 consecutive failures
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEndTime = DateTime.UtcNow.AddMinutes(15);
                    _logger.LogWarning("Account locked after repeated failures for {Email}", email);
                }

                await _dbContext.SaveChangesAsync();
            }

            _logger.LogWarning("Failed login for {Email}", email);
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is inactive.");
        }

        // Optionally block login until email is verified
        var requireEmailVerification = bool.TrueString.Equals(
            _configuration["Auth:RequireEmailVerification"], StringComparison.OrdinalIgnoreCase);
        if (requireEmailVerification && !user.IsEmailVerified)
        {
            throw new UnauthorizedException("Please verify your email address before logging in.");
        }

        // Successful login — reset lockout counters
        user.FailedLoginAttempts = 0;
        user.LockoutEndTime = null;
        user.LastSuccessfulLogin = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        if (dto.IsTwoFactorEnabled)
        {
            await SendOtpAsync(user.Email);
            return new AuthResponseDto
            {
                OtpRequired = true,
                Message = "OTP required. Please verify OTP to continue.",
                User = _mapper.Map<UserResponseDto>(user)
            };
        }

        var roleName = user.Role?.Name ?? "Customer";
        var rawRefreshLogin = _tokenService.GenerateRefreshToken();
        await _tokenService.StoreRefreshTokenAsync(user.Id, rawRefreshLogin);
        return new AuthResponseDto
        {
            Token = await _authService.GenerateJwtToken(user, roleName),
            RefreshToken = rawRefreshLogin,
            OtpRequired = false,
            Message = "Login successful.",
            User = _mapper.Map<UserResponseDto>(user)
        };
    }

    public async Task<bool> SendOtpAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        var recentCount = await _dbContext.OtpVerifications
            .CountAsync(o => o.Email.ToLower() == normalized && o.CreatedAt >= oneMinuteAgo);

        if (recentCount >= 3)
        {
            throw new ValidationException("OTP rate limit exceeded. Try again later.");
        }

        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var otpHash = HashValue(otp);

        var otpRecord = new OtpVerificationModel
        {
            Email = normalized,
            OtpCode = otpHash,
            Purpose = "Login",
            ExpiryTime = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            MaxAttempts = 3,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.OtpVerifications.AddAsync(otpRecord);
        await _dbContext.SaveChangesAsync();

        await _emailService.SendOtpEmailAsync(normalized, otp);
        return true;
    }

    public async Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto dto)
    {
        var normalized = dto.Email.Trim().ToLowerInvariant();
        var otpHash = HashValue(dto.Otp.Trim());

        var otpRecord = await _dbContext.OtpVerifications
            .Where(o => o.Email.ToLower() == normalized && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otpRecord is null)
        {
            _logger.LogWarning("OTP verification failed for {Email}: no record", normalized);
            throw new UnauthorizedException("Invalid OTP.");
        }

        if (otpRecord.ExpiryTime < DateTime.UtcNow)
        {
            _logger.LogWarning("OTP verification failed for {Email}: expired", normalized);
            throw new UnauthorizedException("OTP expired.");
        }

        otpRecord.AttemptCount++;
        if (otpRecord.AttemptCount > otpRecord.MaxAttempts || otpRecord.OtpCode != otpHash)
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogWarning("OTP verification failed for {Email}: invalid", normalized);
            throw new UnauthorizedException("Invalid OTP.");
        }

        otpRecord.IsUsed = true;
        otpRecord.VerifiedAt = DateTime.UtcNow;

        var user = await _dbContext.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized && !u.IsDeleted)
            ?? throw new NotFoundException("User not found.");

        await _dbContext.SaveChangesAsync();

        var roleName = user.Role?.Name ?? "Customer";
        var rawRefreshOtp = _tokenService.GenerateRefreshToken();
        await _tokenService.StoreRefreshTokenAsync(user.Id, rawRefreshOtp);
        return new AuthResponseDto
        {
            Token = await _authService.GenerateJwtToken(user, roleName),
            RefreshToken = rawRefreshOtp,
            OtpRequired = false,
            Message = "OTP verified successfully.",
            User = _mapper.Map<UserResponseDto>(user)
        };
    }

    public async Task<UserResponseDto> GetUserByIdAsync(int userId)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted)
            ?? throw new NotFoundException("User not found.");

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .Include(u => u.Role)
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<UserResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<bool> UpdateUserAsync(int userId, UpdateUserDto dto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted)
            ?? throw new NotFoundException("User not found.");

        if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
        if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
        if (dto.Address is not null) user.Address = dto.Address;
        if (dto.City is not null) user.City = dto.City;
        if (dto.State is not null) user.State = dto.State;
        if (dto.Country is not null) user.Country = dto.Country;
        if (dto.PinCode is not null) user.PinCode = dto.PinCode;
        if (dto.ProfileImageUrl is not null)
        {
            if (!Uri.TryCreate(dto.ProfileImageUrl, UriKind.Absolute, out var parsedUrl)
                || (parsedUrl.Scheme != Uri.UriSchemeHttp && parsedUrl.Scheme != Uri.UriSchemeHttps))
            {
                throw new ValidationException("ProfileImageUrl must be a valid HTTP or HTTPS URL.");
            }
            user.ProfileImageUrl = dto.ProfileImageUrl;
        }
        if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;

        if (dto.RoleId.HasValue)
        {
            if (!dto.IsAdminAction)
            {
                throw new UnauthorizedException("Only admins can change user roles.");
            }

            var roleExists = await _dbContext.Roles.AnyAsync(r => r.Id == dto.RoleId.Value && r.IsActive);
            if (!roleExists)
            {
                throw new ValidationException("Invalid role.");
            }

            user.RoleId = dto.RoleId.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted)
            ?? throw new NotFoundException("User not found.");

        user.IsDeleted = true;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static string HashValue(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    public async Task<AuthResponseDto> RefreshAccessTokenAsync(int userId, string rawRefreshToken)
    {
        var user = await _dbContext.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted)
            ?? throw new UnauthorizedException("User not found.");

        if (!user.IsActive)
            throw new UnauthorizedException("User account is inactive.");

        // Rotate: revoke old, issue new
        await _tokenService.RevokeRefreshTokenAsync(userId, rawRefreshToken);
        var newRawRefresh = _tokenService.GenerateRefreshToken();
        await _tokenService.StoreRefreshTokenAsync(userId, newRawRefresh);

        var roleName = user.Role?.Name ?? "Customer";
        return new AuthResponseDto
        {
            Token        = await _authService.GenerateJwtToken(user, roleName),
            RefreshToken = newRawRefresh,
            OtpRequired  = false,
            Message      = "Token refreshed.",
            User         = _mapper.Map<UserResponseDto>(user)
        };
    }

    public async Task<bool> VerifyEmailAsync(string token)    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token && !u.IsDeleted)
            ?? throw new NotFoundException("Invalid or expired verification link.");

        if (user.EmailVerificationExpiry < DateTime.UtcNow)
            throw new ValidationException("Verification link has expired. Please request a new one.");

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResendVerificationEmailAsync(string email)    {
        var normalized = email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized && !u.IsDeleted)
            ?? throw new NotFoundException("No account found with that email address.");

        if (user.IsEmailVerified)
            throw new ValidationException("Email is already verified.");

        var rawToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        user.EmailVerificationToken = rawToken;
        user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5000";
        var verifyLink = $"{baseUrl}/api/auth/verify-email?token={Uri.EscapeDataString(rawToken)}";
        await _emailService.SendVerificationEmailAsync(normalized, user.FirstName, verifyLink);
        return true;
    }
}
