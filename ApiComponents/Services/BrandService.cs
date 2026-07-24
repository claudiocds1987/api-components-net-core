//using ApiComponents.Domain.Models;
//using ApiComponents.Application.Repositories;

//namespace ApiComponents.Services
//{
//    // Usando Constructor Principal
//    public class BrandService(IBrandRepository brandRepo) : IBrandService
//    {
//        public async Task<IEnumerable<ProductBrand>> GetAllBrandsAsync(bool? isActive = true)
//        {
//            return await brandRepo.GetAllBrandsAsync(isActive);
//        }

//        public async Task<ProductBrand?> GetBrandByIdAsync(int id) => await brandRepo.GetBrandAsync(id);

//        public async Task CreateBrandAsync(ProductBrand brand)
//        {
//            if (await brandRepo.ExistBrand(brand.name))
//                throw new ApplicationException("La marca ya existe.");

//            await brandRepo.AddBrand(brand);
//        }

//        public async Task UpdateBrandAsync(ProductBrand brand) => await brandRepo.UpdateBrandAsync(brand);

//        public async Task DeleteBrandAsync(int id) => await brandRepo.DeleteBrandAsync(id);
//    }
//}