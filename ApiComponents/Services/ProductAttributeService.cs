using ApiComponents.DTOs;
using ApiComponents.Persistence.Repositories;

namespace ApiComponents.Services
{
    public class ProductAttributeService(IProductAttributeRepository repo) : IProductAttributeService
    {
        public async Task<IEnumerable<ProductExtraAttributesDto>> GetAttributesByCategoryId(int categoryId)
        {
            return await repo.GetByCategoryId(categoryId);
        }
    }
}
