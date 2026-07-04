using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using ApiComponents.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db) => _db = db;

    public async Task AddProductsList(List<object> products, CancellationToken cancellationToken = default)
    {
        // Minimal implementation placeholder
        await Task.CompletedTask;
    }

    public async Task<bool> ExistProduct(string title, CancellationToken cancellationToken = default)
    {
        return await _db.Products.AnyAsync(p => p.title == title, cancellationToken);
    }

    public async Task<ProductResponseDto?> GetProduct(int id, CancellationToken cancellationToken = default)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.id == id, cancellationToken);
        if (p == null) return null;
        return new ProductResponseDto
        {
            id = p.id ?? 0,
            title = p.title,
            description = p.description,
            price = p.price,
            discountPercentage = p.discountPercentage,
            rating = p.rating,
            stock = p.stock,
            sku = p.sku,
            weight = p.weight,
            width = p.width,
            height = p.height,
            depth = p.depth,
            warrantyInformation = p.warrantyInformation,
            shippingInformation = p.shippingInformation,
            availabilityStatus = p.availabilityStatus,
            returnPolicy = p.returnPolicy,
            minimumOrderQuantity = p.minimumOrderQuantity,
            thumbnail = p.thumbnail,
            categoryId = p.categoryId,
            brandId = p.brandId,
            isActive = p.isActive
        };
    }

    public async Task<(List<object> Items, int TotalCount)> GetProductsAsync(int? page, int? size, string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, string? sortBy, string? order, bool? isActive = true, CancellationToken cancellationToken = default)
    {
        var query = _db.Products.AsQueryable();
        int total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(((page ?? 1) - 1) * (size ?? 25)).Take(size ?? 25).ToListAsync(cancellationToken);
        return (Items: items.Cast<object>().ToList(), TotalCount: total);
    }

    public async Task<(List<ApiComponents.Application.DTOs.ProductAdminDto> Items, int TotalCount)> GetProductsAdminAsync(int? page, int? size, string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, string? sortBy, string? order, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Products.AsQueryable();
        int total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(((page ?? 1) - 1) * (size ?? 25)).Take(size ?? 25).Select(p => new ApiComponents.Application.DTOs.ProductAdminDto { id = p.id ?? 0, title = p.title, sku = p.sku, price = p.price, stock = p.stock, categoryId = p.categoryId, brandId = p.brandId, isActive = p.isActive, imageUrl = p.thumbnail }).ToListAsync(cancellationToken);
        return (Items: items, TotalCount: total);
    }

    public async Task CreateProduct(ProductRequestDTo productDto, string scheme, string host, CancellationToken cancellationToken = default)
    {
        var product = new ApiComponents.Domain.Models.Product
        {
            title = productDto.title,
            description = productDto.description,
            price = productDto.price,
            discountPercentage = productDto.discountPercentage,
            rating = productDto.rating,
            stock = productDto.stock,
            sku = productDto.sku,
            weight = productDto.weight,
            width = productDto.width,
            height = productDto.height,
            depth = productDto.depth,
            warrantyInformation = productDto.warrantyInformation,
            shippingInformation = productDto.shippingInformation,
            availabilityStatus = productDto.availabilityStatus,
            returnPolicy = productDto.returnPolicy,
            minimumOrderQuantity = productDto.minimumOrderQuantity,
            brandId = productDto.brandId,
            categoryId = productDto.categoryId,
            isActive = productDto.isActive,
            thumbnail = productDto.thumbnail
        };

        product.ValidateForCreate();

        await _db.Products.AddAsync(product, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductRequestDTo> UpdateProduct(ProductRequestDTo productDto, string scheme, string host, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.id == productDto.id, cancellationToken);
        if (existing == null) throw new Exception("Producto no encontrado");

        existing.title = productDto.title;
        existing.description = productDto.description;
        existing.price = productDto.price;
        existing.discountPercentage = productDto.discountPercentage;
        existing.rating = productDto.rating;
        existing.stock = productDto.stock;
        existing.sku = productDto.sku;
        existing.weight = productDto.weight;
        existing.width = productDto.width;
        existing.height = productDto.height;
        existing.depth = productDto.depth;
        existing.warrantyInformation = productDto.warrantyInformation ?? string.Empty;
        existing.shippingInformation = productDto.shippingInformation ?? string.Empty;
        existing.availabilityStatus = productDto.availabilityStatus;
        existing.returnPolicy = productDto.returnPolicy ?? string.Empty;
        existing.minimumOrderQuantity = productDto.minimumOrderQuantity;
        existing.categoryId = productDto.categoryId;
        existing.brandId = productDto.brandId;
        existing.isActive = productDto.isActive;

        await _db.SaveChangesAsync(cancellationToken);

        return productDto;
    }

    public async Task<ProductRequestDTo> UpdateProductStatus(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.id == id, cancellationToken);
        if (existing == null) throw new Exception("Producto no encontrado");
        existing.ChangeActiveState(isActive);
        await _db.SaveChangesAsync(cancellationToken);
        return new ProductRequestDTo { id = existing.id, title = existing.title, isActive = existing.isActive };
    }
}
