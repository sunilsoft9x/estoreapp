using Microsoft.EntityFrameworkCore;
using MyEstore.Data;
using MyEstore.DTOs;
using MyEstore.Exceptions;
using MyEstore.Models;
using MyEstore.Services.Interfaces;

namespace MyEstore.Services.Implementations;

public class WishlistService : IWishlistService
{
    private readonly AppDbContext _dbContext;

    public WishlistService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<WishlistItemResponseDto>> GetWishlistAsync(int userId)
    {
        return await _dbContext.WishlistItems
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Include(w => w.Product)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WishlistItemResponseDto
            {
                Id          = w.Id,
                ProductId   = w.ProductId,
                ProductName = w.Product!.Name,
                Price       = w.Product.Price,
                Discount    = w.Product.Discount,
                ImageUrl    = w.Product.ImageUrl,
                InStock     = w.Product.StockQuantity > 0 && w.Product.IsActive && !w.Product.IsDeleted,
                AddedAt     = w.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<WishlistItemResponseDto> AddToWishlistAsync(int userId, int productId)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted)
            ?? throw new NotFoundException($"Product {productId} not found.");

        var existing = await _dbContext.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (existing is not null)
        {
            // Already in wishlist — return current entry rather than throwing
            return new WishlistItemResponseDto
            {
                Id          = existing.Id,
                ProductId   = product.Id,
                ProductName = product.Name,
                Price       = product.Price,
                Discount    = product.Discount,
                ImageUrl    = product.ImageUrl,
                InStock     = product.StockQuantity > 0 && product.IsActive && !product.IsDeleted,
                AddedAt     = existing.CreatedAt
            };
        }

        var item = new WishlistItemModel
        {
            UserId    = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.WishlistItems.AddAsync(item);
        await _dbContext.SaveChangesAsync();

        return new WishlistItemResponseDto
        {
            Id          = item.Id,
            ProductId   = product.Id,
            ProductName = product.Name,
            Price       = product.Price,
            Discount    = product.Discount,
            ImageUrl    = product.ImageUrl,
            InStock     = product.StockQuantity > 0 && product.IsActive && !product.IsDeleted,
            AddedAt     = item.CreatedAt
        };
    }

    public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
    {
        var item = await _dbContext.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId)
            ?? throw new NotFoundException("Item not in wishlist.");

        _dbContext.WishlistItems.Remove(item);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsInWishlistAsync(int userId, int productId)
        => await _dbContext.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
}
