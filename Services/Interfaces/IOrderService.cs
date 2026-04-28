using MyEstore.DTOs;

namespace MyEstore.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(int userId, CreateOrderDto dto);
    Task<OrderResponseDto> GetOrderByIdAsync(int orderId, int requestingUserId, bool isAdmin);
    Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(int userId);
    Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(int page = 1, int pageSize = 20);
    Task<bool> UpdateOrderStatusAsync(int orderId, string status);
    Task<bool> CancelOrderAsync(int orderId, int requestingUserId, bool isAdmin);
}
