using Microsoft.AspNetCore.Mvc;
using MyEstore.DTOs;
using MyEstore.Services.Interfaces;

namespace MyEstore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ITokenService tokenService, ILogger<AuthController> logger)
    {
        _userService  = userService;
        _tokenService = tokenService;
        _logger       = logger;
    }

    // POST api/auth/register
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.RegisterAsync(dto);
        return CreatedAtAction(nameof(Register), result);
    }

    // POST api/auth/login
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.LoginAsync(dto);
        return Ok(result);
    }

    // POST api/auth/refresh
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Decode userId from the raw refresh token prefix is not possible — the token itself
        // carries no user info. We scan the DB for a matching hash across active tokens.
        // For security we return 401 on any mismatch (no distinguishing detail).
        var userId = await _tokenService.GetUserIdByRefreshTokenAsync(dto.RefreshToken);
        if (userId is null)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        var isValid = await _tokenService.ValidateRefreshTokenAsync(userId.Value, dto.RefreshToken);
        if (!isValid)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        var result = await _userService.RefreshAccessTokenAsync(userId.Value, dto.RefreshToken);
        return Ok(result);
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        // Best-effort revoke — even if token not found we return 200 (idempotent logout)
        if (!string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            var userId = await _tokenService.GetUserIdByRefreshTokenAsync(dto.RefreshToken);
            if (userId is not null)
                await _tokenService.RevokeRefreshTokenAsync(userId.Value, dto.RefreshToken);
        }
        return Ok(new { message = "Logged out successfully." });
    }

    // GET api/auth/verify-email?token=...
    [HttpGet("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "Verification token is required." });

        await _userService.VerifyEmailAsync(token);
        return Ok(new { message = "Email verified successfully. You can now log in." });
    }

    // POST api/auth/resend-verification?email=...
    [HttpPost("resend-verification")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendVerification([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required." });

        await _userService.ResendVerificationEmailAsync(email);
        return Ok(new { message = "Verification email sent. Please check your inbox." });
    }

    // POST api/auth/send-otp
    [HttpPost("send-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendOtp([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required." });

        var sent = await _userService.SendOtpAsync(email);
        if (!sent)
            return NotFound(new { message = "No account found with that email address." });

        return Ok(new { message = "OTP sent successfully. Please check your email." });
    }

    // POST api/auth/verify-otp
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.VerifyOtpAsync(dto);
        return Ok(result);
    }
}
