namespace ApiComponents.Services;

using ApiComponents.DTOs;

public interface IProductService
{
    Task<ImportResultDto> ProcessCsvAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<ProductResponseDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

    // endpoint pesado para el E-commerce
    Task<object> GetAllProductsAsync(
         int? page,
         int? size,
         string? search = null,
         int? categoryId = null,
         int? brandId = null,
         decimal? minPrice = null,
         decimal? maxPrice = null,
         string? sortBy = "title",
         string? order = "asc",
         bool? isActive = true,
         CancellationToken cancellationToken = default);

    // endpoint liviano para la Grilla Admin
    Task<object> GetProductsAdminAsync(
         int? page,
         int? size,
         string? search = null,
         int? categoryId = null,
         int? brandId = null,
         decimal? minPrice = null,
         decimal? maxPrice = null,
         string? sortBy = "title",
         string? order = "asc",
         bool? isActive = null, // "null" por que el admin suele querer ver todos por defecto activos e inactivos
         CancellationToken cancellationToken = default);

    Task CreateProductAsync(ProductRequestDTo product, string scheme, string host, CancellationToken cancellationToken = default);
    Task<ProductRequestDTo> UpdateProductAsync(ProductRequestDTo product, string scheme, string host, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}