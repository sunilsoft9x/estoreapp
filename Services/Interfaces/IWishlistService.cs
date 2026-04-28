using MyEstore.DTOs;

namespace MyEstore.Services.Interfaces;

public interface IWishlistService
{
    Task<IEnumerable<WishlistItemResponseDto>> GetWishlistAsync(int userId);
    Task<WishlistItemResponseDto> AddToWishlistAsync(int userId, int productId);
    Task<bool> RemoveFromWishlistAsync(int userId, int productId);
    Task<bool> IsInWishlistAsync(int userId, int productId);
}
