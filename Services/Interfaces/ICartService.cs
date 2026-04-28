using MyEstore.DTOs;

namespace MyEstore.Services.Interfaces;

public interface ICartService
{
    Task<CartResponseDto> GetCartAsync(int userId);
    Task<CartResponseDto> AddToCartAsync(int userId, AddToCartDto dto);
    Task<CartResponseDto> UpdateCartItemAsync(int userId, int productId, int quantity);
    Task<bool> RemoveFromCartAsync(int userId, int productId);
    Task<bool> ClearCartAsync(int userId);
}
