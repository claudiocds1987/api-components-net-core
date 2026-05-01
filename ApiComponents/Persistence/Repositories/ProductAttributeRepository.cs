using ApiComponents.DTOs;
using ApiComponents.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Persistence.Repositories
{
    public class ProductAttributeRepository(AppDbContext db) : IProductAttributeRepository
    {
        public async Task<IEnumerable<ProductExtraAttributesDto>> GetByCategoryId(int categoryId)
        {
            return await db.ProductAttributeDefinitions
                .Where(ad => ad.categoryId == categoryId)
                .Select(ad => new ProductExtraAttributesDto
                {
                    name = ad.name.ToLower(),
                    label = ad.name,
                    dataType = ad.dataType,
                    required = true // Puedes mapear esto desde una columna real si la agregas a la DB
                })
                .ToListAsync();
        }
    }
}
