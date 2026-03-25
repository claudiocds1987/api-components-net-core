using ApiComponents.Models;

namespace ApiComponents.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync(bool? isActive = true);
        Task<ProductCategory?> GetCategoryByIdAsync(int id);
        Task CreateCategoryAsync(ProductCategory category);
        Task UpdateCategoryAsync(ProductCategory category);
        Task DeleteCategoryAsync(int id);
    }
}
