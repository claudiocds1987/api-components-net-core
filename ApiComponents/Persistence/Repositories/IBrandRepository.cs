using ApiComponents.Models;

namespace ApiComponents.Persistence.Repositories
{
    public interface IBrandRepository
    {
        Task<IEnumerable<ProductBrand>> GetAllBrands(bool? isActive = true);
        Task<ProductBrand?> GetBrand(int id);
        Task AddBrand(ProductBrand brand);
        Task UpdateBrand(ProductBrand brand);
        Task DeleteBrand(int id);
        Task<bool> ExistBrand(string name);
    }
}
