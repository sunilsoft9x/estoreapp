using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEstore.DTOs;
using MyEstore.Services.Interfaces;
using System.Security.Claims;

namespace MyEstore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    // POST api/coupons — Admin only
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CouponResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _couponService.CreateCouponAsync(dto);
        return CreatedAtAction(nameof(GetAllCoupons), result);
    }

    // GET api/coupons — Admin only
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<CouponResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCoupons()
    {
        var coupons = await _couponService.GetAllCouponsAsync();
        return Ok(coupons);
    }

    // POST api/coupons/validate — Authenticated users
    [HttpPost("validate")]
    [Authorize]
    [ProducesResponseType(typeof(ValidateCouponResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateCoupon([FromQuery] string code, [FromQuery] decimal orderSubtotal)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest(new { message = "Coupon code is required." });

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _couponService.ValidateCouponAsync(code, userId, orderSubtotal);
        return Ok(result);
    }

    // DELETE api/coupons/{id} — Admin only
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateCoupon(int id)
    {
        await _couponService.DeactivateCouponAsync(id);
        return NoContent();
    }
}
