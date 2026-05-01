using ApiComponents.DTOs;

namespace ApiComponents.Persistence.Repositories
{
    public interface IProductAttributeRepository
    {
        Task<IEnumerable<ProductExtraAttributesDto>> GetByCategoryId(int categoryId);
    }
}