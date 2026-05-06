using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Persistence.Repositories
{
    public class ProductAttributeRepository(AppDbContext db) : IProductAttributeRepository
    {
        public async Task<IEnumerable<ProductExtraAttributeDefinition>> GetExtraAttributesByCategoryId(int categoryId)
        {
            // Ahora devolvemos la lista de ENTIDADES directamente desde la DB
            return await db.ProductAttributeDefinitions
                .Where(ad => ad.categoryId == categoryId)
                .ToListAsync();
        }

        public async Task AddExtraAttributes(ProductExtraAttributeDefinition attribute)
        {
            await db.ProductAttributeDefinitions.AddAsync(attribute);
        }

        public void UpdateExtraAttributes(ProductExtraAttributeDefinition attribute)
        {
            // EF Core rastrea las entidades obtenidas, pero esto asegura el estado 'Modified'
            db.ProductAttributeDefinitions.Update(attribute);
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
