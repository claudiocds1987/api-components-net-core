namespace ApiComponents.Services;

using ApiComponents.DTOs;
using ApiComponents.Models;

public interface IProductService
{
    Task<ImportResultDto> ProcessCsvAsync(IFormFile file);
    Task<Product?> GetProductByIdAsync(int id);
    Task<object> GetAllProductsAsync(
         int? page,
         int? size,
         string? search = null,
         int? categoryId = null,
         decimal? minPrice = null,
         decimal? maxPrice = null,
         string? sortBy = "title",
         string? order = "asc");
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
}