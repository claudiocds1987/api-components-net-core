using ApiComponents.Domain.Models;

namespace ApiComponents.Application.Repositories
{
    public interface ICategoryRepository
    {
        // Modificado para aceptar el filtro opcional
        Task<IEnumerable<ProductCategory>> GetAllAsync(bool? isActive = true, CancellationToken cancellationToken = default);
        Task<ProductCategory?> GetCategoryAsync(int id, CancellationToken cancellationToken = default);
        Task AddCategoryAsync(ProductCategory category, CancellationToken cancellationToken = default);
        Task UpdateCategoryAsync(ProductCategory category, CancellationToken cancellationToken = default);
        Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistCategoryAsync(string name, CancellationToken cancellationToken = default);
    }
}
