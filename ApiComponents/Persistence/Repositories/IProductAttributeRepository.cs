using ApiComponents.Models;

namespace ApiComponents.Persistence.Repositories
{
    public interface IProductAttributeRepository
    {
        Task<IEnumerable<ProductExtraAttributeDefinition>> GetExtraAttributesByCategoryId(int categoryId);
        Task AddExtraAttributes(ProductExtraAttributeDefinition attribute);
        void UpdateExtraAttributes(ProductExtraAttributeDefinition attribute);
        Task SaveChangesAsync();
    }
}