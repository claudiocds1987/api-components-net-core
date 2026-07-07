using ApiComponents.Domain.Models;

namespace ApiComponents.Application.Repositories
{
    public interface IProductAttributeRepository
    {
        Task<IEnumerable<ProductExtraAttributeDefinition>> GetExtraAttributesByCategoryId(int categoryId);
        Task AddExtraAttributes(ProductExtraAttributeDefinition attribute);
        void UpdateExtraAttributes(ProductExtraAttributeDefinition attribute);

        Task<ProductExtraAttributeDefinition?> GetDefinitionByIdAsync(int id);
        void RemoveExtraAttribute(ProductExtraAttributeDefinition extraAttribute);
        Task SaveChangesAsync();
    }
}