using ApiComponents.DTOs;

namespace ApiComponents.Services
{
    public interface IProductExtraAttributeService
    {
        Task<IEnumerable<ProductExtraAttributesDto>> GetExtraAttributesByCategoryId(int categoryId);
        Task SaveExtraAttributes(int categoryId, List<ProductExtraAttributesDto> attributes);
        Task DeleteExtraAttributeAsync(int extraAttributeId);
    }
}
