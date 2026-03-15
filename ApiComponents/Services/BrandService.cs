using ApiComponents.Models;
using ApiComponents.Persistence.Repositories;

namespace ApiComponents.Services
{
    // Usando Constructor Principal
    public class BrandService(IBrandRepository brandRepo) : IBrandService
    {
        public async Task<IEnumerable<ProductBrand>> GetAllBrandsAsync() => await brandRepo.GetAllBrands();

        public async Task<ProductBrand> GetBrandByIdAsync(int id) => await brandRepo.GetBrand(id);

        public async Task CreateBrandAsync(ProductBrand brand)
        {
            if (await brandRepo.ExistBrand(brand.name))
                throw new ApplicationException("La marca ya existe.");

            await brandRepo.AddBrand(brand);
        }

        public async Task UpdateBrandAsync(ProductBrand brand) => await brandRepo.UpdateBrand(brand);

        public async Task DeleteBrandAsync(int id) => await brandRepo.DeleteBrand(id);
    }
}