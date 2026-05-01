using ApiComponents.DTOs;

namespace ApiComponents.Services
{
    public interface IProductAttributeService
    {
        Task<IEnumerable<ProductExtraAttributesDto>> GetAttributesByCategoryId(int categoryId);
    }
}
