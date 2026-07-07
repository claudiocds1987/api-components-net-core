using ApiComponents.Application.DTOs;
using ApiComponents.Domain.Models;

namespace ApiComponents.Application.Repositories;

public interface IProductRepository
{
    Task AddProductsList(List<Product> products, CancellationToken cancellationToken = default);
    Task<bool> ExistProduct(string title, CancellationToken cancellationToken = default);
    Task<ProductResponseDto?> GetProduct(int id, CancellationToken cancellationToken = default);
    // para soportar filtros, ordenamiento y paginación
    Task<(List<Product> Items, int TotalCount)> GetProductsAsync(
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
    Task<(List<ProductAdminDto> Items, int TotalCount)> GetProductsAdminAsync(
           int? page,
           int? size,
           string? search = null,
           int? categoryId = null,
           int? brandId = null,
           decimal? minPrice = null,
           decimal? maxPrice = null,
           string? sortBy = "title",
           string? order = "asc",
           bool? isActive = null, // El admin suele querer ver todos por defecto activos e inactivos
           CancellationToken cancellationToken = default);
    Task CreateProduct(ProductRequestDTo productDto, string scheme, string host, CancellationToken cancellationToken = default);
    Task<ProductRequestDTo> UpdateProduct(ProductRequestDTo productDto, string scheme, string host, CancellationToken cancellationToken = default);
    Task<ProductRequestDTo> UpdateProductStatus(int id, bool isActive, CancellationToken cancellationToken = default);
}
