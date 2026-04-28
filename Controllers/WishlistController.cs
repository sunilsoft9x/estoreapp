using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEstore.DTOs;
using MyEstore.Services.Interfaces;
using System.Security.Claims;

namespace MyEstore.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET api/wishlist
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WishlistItemResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWishlist()
    {
        var items = await _wishlistService.GetWishlistAsync(UserId);
        return Ok(items);
    }

    // POST api/wishlist/{productId}
    [HttpPost("{productId:int}")]
    [ProducesResponseType(typeof(WishlistItemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToWishlist(int productId)
    {
        var item = await _wishlistService.AddToWishlistAsync(UserId, productId);
        return CreatedAtAction(nameof(GetWishlist), item);
    }

    // DELETE api/wishlist/{productId}
    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        await _wishlistService.RemoveFromWishlistAsync(UserId, productId);
        return NoContent();
    }

    // GET api/wishlist/{productId}/check
    [HttpGet("{productId:int}/check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckInWishlist(int productId)
    {
        var inWishlist = await _wishlistService.IsInWishlistAsync(UserId, productId);
        return Ok(new { productId, inWishlist });
    }
}
