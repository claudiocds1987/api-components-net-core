using ApiComponents.Application.DTOs;

namespace ApiComponents.Application.Repositories;

public interface IProductRepository
{
    Task AddProductsList(List<object> products, CancellationToken cancellationToken = default);
    Task<bool> ExistProduct(string title, CancellationToken cancellationToken = default);
    Task<ProductResponseDto?> GetProduct(int id, CancellationToken cancellationToken = default);
    Task<(List<object> Items, int TotalCount)> GetProductsAsync(int? page, int? size, string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, string? sortBy, string? order, bool? isActive = true, CancellationToken cancellationToken = default);
    Task<(List<ApiComponents.Application.DTOs.ProductAdminDto> Items, int TotalCount)> GetProductsAdminAsync(int? page, int? size, string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, string? sortBy, string? order, bool? isActive = null, CancellationToken cancellationToken = default);
    Task CreateProduct(ProductRequestDTo productDto, string scheme, string host, CancellationToken cancellationToken = default);
    Task<ProductRequestDTo> UpdateProduct(ProductRequestDTo productDto, string scheme, string host, CancellationToken cancellationToken = default);
    Task<ProductRequestDTo> UpdateProductStatus(int id, bool isActive, CancellationToken cancellationToken = default);
}
