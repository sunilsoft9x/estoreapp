using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEstore.DTOs;
using MyEstore.Services.Interfaces;

namespace MyEstore.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    // GET api/cart
    [HttpGet]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var cart = await _cartService.GetCartAsync(userId.Value);
        return Ok(cart);
    }

    // POST api/cart
    [HttpPost]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var cart = await _cartService.AddToCartAsync(userId.Value, dto);
        return Ok(cart);
    }

    // PUT api/cart/{productId}?quantity=3
    [HttpPut("{productId:int}")]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCartItem(int productId, [FromQuery] int quantity)
    {
        if (quantity < 1)
            return BadRequest(new { message = "Quantity must be at least 1. To remove an item, use the DELETE endpoint." });

        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var cart = await _cartService.UpdateCartItemAsync(userId.Value, productId, quantity);
        return Ok(cart);
    }

    // DELETE api/cart/{productId}
    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var removed = await _cartService.RemoveFromCartAsync(userId.Value, productId);
        if (!removed)
            return NotFound(new { message = "Item not found in cart." });

        return NoContent();
    }

    // DELETE api/cart
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        await _cartService.ClearCartAsync(userId.Value);
        return NoContent();
    }

    // ─── helpers ─────────────────────────────────────────────────────────────

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue("id");
        return int.TryParse(value, out var id) ? id : null;
    }
}
