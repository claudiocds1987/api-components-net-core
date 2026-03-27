using ApiComponents.DTOs;
using ApiComponents.Models;

namespace ApiComponents.Persistence.Repositories
{
    public interface IProductRepository
    {

        Task AddProductsList(List<Product> products);
        Task<Product?> GetProduct(int id);
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
            bool? isActive = true);

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
            bool? isActive = null); // El admin suele querer ver todos por defecto activos e inactivos
        Task DeleteProduct(int id);
        Task UpdateProduct(Product product);
        Task<bool> ExistProduct(string title); // Validamos por título ya que el ID es automático
    }
}
