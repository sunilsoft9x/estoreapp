using MyEstore.DTOs;

namespace MyEstore.Services.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductResponseDto> UpdateProductAsync(int productId, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int productId);
    Task<ProductResponseDto> GetProductByIdAsync(int productId);
    Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(int page = 1, int pageSize = 20);
    Task<IEnumerable<ProductResponseDto>> SearchProductsAsync(string query);
    Task<IEnumerable<ProductResponseDto>> GetProductsByCategoryAsync(string category);
    Task<bool> UpdateStockAsync(int productId, int quantity);
}
