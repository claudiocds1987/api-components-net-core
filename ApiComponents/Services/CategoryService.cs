using ApiComponents.Models;
using ApiComponents.Persistence.Repositories;

namespace ApiComponents.Services
{
    // Usando Constructor Principal
    public class CategoryService(ICategoryRepository categoryRepo) : ICategoryService
    {
        public async Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync() => await categoryRepo.GetAllCategories();

        public async Task<ProductCategory?> GetCategoryByIdAsync(int id) => await categoryRepo.GetCategory(id);

        public async Task CreateCategoryAsync(ProductCategory category)
        {
            if (await categoryRepo.ExistCategory(category.name))
                throw new ApplicationException("La categoría ya existe.");

            await categoryRepo.AddCategory(category);
        }

        public async Task UpdateCategoryAsync(ProductCategory category) => await categoryRepo.UpdateCategory(category);

        public async Task DeleteCategoryAsync(int id) => await categoryRepo.DeleteCategory(id);
    }
}