using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MyEstore.Data;
using MyEstore.DTOs;
using MyEstore.Exceptions;
using MyEstore.Models;
using MyEstore.Services.Interfaces;

namespace MyEstore.Services.Implementations;

public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public ProductService(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto)
    {
        if (dto.StockQuantity < 0)
        {
            throw new ValidationException("Stock cannot be negative.");
        }

        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.IsActive && !c.IsDeleted);
        if (category is null)
        {
            throw new NotFoundException("Category not found.");
        }

        var product = new ProductModel
        {
            Name = dto.Name,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Price = dto.Price,
            Discount = dto.DiscountedPrice,
            StockQuantity = dto.StockQuantity,
            ImageUrl = dto.ImageUrls?.FirstOrDefault(),
            IsFeatured = dto.IsFeatured,
            IsActive = dto.IsActive,
            IsDeleted = false
        };

        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        await _dbContext.Entry(product).Reference(p => p.Category).LoadAsync();
        return _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<ProductResponseDto> UpdateProductAsync(int productId, UpdateProductDto dto)
    {
        var product = await _dbContext.Products.Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted)
            ?? throw new NotFoundException("Product not found.");

        if (dto.StockQuantity.HasValue && dto.StockQuantity.Value < 0)
        {
            throw new ValidationException("Stock cannot be negative.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Name)) product.Name = dto.Name;
        if (dto.Description is not null) product.Description = dto.Description;
        if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.Discount.HasValue) product.Discount = dto.Discount.Value;
        if (dto.StockQuantity.HasValue) product.StockQuantity = dto.StockQuantity.Value;
        if (dto.ImageUrl is not null) product.ImageUrl = dto.ImageUrl;
        if (dto.IsFeatured.HasValue) product.IsFeatured = dto.IsFeatured.Value;
        if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;

        await _dbContext.SaveChangesAsync();
        return _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted)
            ?? throw new NotFoundException("Product not found.");

        product.IsDeleted = true;
        product.IsActive = false;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<ProductResponseDto> GetProductByIdAsync(int productId)
    {
        var product = await _dbContext.Products.AsNoTracking().Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted)
            ?? throw new NotFoundException("Product not found.");

        return _mapper.Map<ProductResponseDto>(product);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return await _dbContext.Products.AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive)
            .Include(p => p.Category)
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<ProductResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductResponseDto>> SearchProductsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<ProductResponseDto>();
        }

        query = query.Trim().ToLowerInvariant();

        return await _dbContext.Products.AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive &&
                (p.Name.ToLower().Contains(query) || (p.Description != null && p.Description.ToLower().Contains(query))))
            .Include(p => p.Category)
            .ProjectTo<ProductResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductResponseDto>> GetProductsByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return Array.Empty<ProductResponseDto>();
        }

        var normalized = category.Trim().ToLowerInvariant();

        return await _dbContext.Products.AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive && p.Category != null && p.Category.Name.ToLower() == normalized)
            .Include(p => p.Category)
            .ProjectTo<ProductResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<bool> UpdateStockAsync(int productId, int quantity)
    {
        if (quantity < 0)
        {
            throw new ValidationException("Stock cannot be negative.");
        }

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted)
            ?? throw new NotFoundException("Product not found.");

        product.StockQuantity = quantity;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
