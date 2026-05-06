using ApiComponents.DTOs;

namespace ApiComponents.Services
{
    public interface IProductAttributeService
    {
        Task<IEnumerable<ProductExtraAttributesDto>> GetExtraAttributesByCategoryId(int categoryId);
        Task SaveExtraAttributes(int categoryId, List<ProductExtraAttributesDto> attributes);
    }
}
