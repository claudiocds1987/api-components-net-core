using ApiComponents.Models;

namespace ApiComponents.Persistence.Repositories
{
    public interface ICategoryRepository
    {
        // Modificado para aceptar el filtro opcional
        Task<IEnumerable<ProductCategory>> GetAllCategories(bool? isActive = true);
        Task<ProductCategory?> GetCategory(int id);
        Task AddCategory(ProductCategory category);
        Task UpdateCategory(ProductCategory category);
        Task DeleteCategory(int id);
        Task<bool> ExistCategory(string name);
    }
}
