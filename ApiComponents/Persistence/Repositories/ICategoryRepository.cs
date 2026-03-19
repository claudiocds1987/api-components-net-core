using ApiComponents.Models;

namespace ApiComponents.Persistence.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<ProductCategory>> GetAllCategories();
        Task<ProductCategory?> GetCategory(int id);
        Task AddCategory(ProductCategory category);
        Task UpdateCategory(ProductCategory category);
        Task DeleteCategory(int id);
        Task<bool> ExistCategory(string name); // Validación por nombre
    }
}
