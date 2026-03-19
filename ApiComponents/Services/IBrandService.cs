using ApiComponents.Models;

namespace ApiComponents.Services
{
    public interface IBrandService
    {
        Task<IEnumerable<ProductBrand>> GetAllBrandsAsync();
        Task<ProductBrand?> GetBrandByIdAsync(int id);
        Task CreateBrandAsync(ProductBrand brand);
        Task UpdateBrandAsync(ProductBrand brand);
        Task DeleteBrandAsync(int id);
    }
}
