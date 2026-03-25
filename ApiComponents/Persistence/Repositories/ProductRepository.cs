using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Persistence.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task AddProductsList(List<Product> products)
    {
        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistProduct(string title)
    => await db.Products.AnyAsync(p => EF.Functions.Like(p.title, title));

    public async Task<Product?> GetProduct(int id)
    {
        return await db.Products
          .Include(p => p.images)
          .Include(p => p.tags)
          .Include(p => p.reviews)
          .FirstOrDefaultAsync(p => p.id == id);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetProductsAsync(
     int? page,
     int? size,
     string? search,
     int? categoryId,
     decimal? minPrice,
     decimal? maxPrice,
     string? sortBy,
     string? order)
    {
        var query = db.Products.AsQueryable();

        // 1. Filtrado por Texto
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.title.Contains(search) || p.description.Contains(search));

        // 2. Filtrado por Categoría
        if (categoryId.HasValue && categoryId > 0)
            query = query.Where(p => p.categoryId == categoryId);

        // 3. Filtrado por Rango de Precio
        if (minPrice.HasValue)
            query = query.Where(p => p.price >= minPrice);

        if (maxPrice.HasValue)
            query = query.Where(p => p.price <= maxPrice);

        // 4. Conteo Total (Importante hacerlo antes de la paginación)
        int totalCount = await query.CountAsync();

        // 5. Ordenamiento Dinámico
        if (sortBy?.ToLower() == "price")
        {
            query = order?.ToLower() == "desc"
                ? query.OrderByDescending(p => p.price)
                : query.OrderBy(p => p.price);
        }
        else
        {
            query = order?.ToLower() == "desc"
                ? query.OrderByDescending(p => p.title)
                : query.OrderBy(p => p.title);
        }

        // 6. Paginación y ejecución de la consulta
        // Usamos .ToListAsync() para asegurar que el retorno sea un List<Product>
        var items = await query
            .Skip(((page ?? 1) - 1) * (size ?? 10))
            .Take(size ?? 10)
            .ToListAsync();

        return (Items: items, TotalCount: totalCount);
    }

    public async Task UpdateProduct(Product product)
    {
        db.Entry(product).State = EntityState.Modified;
        await db.SaveChangesAsync();
    }

    public async Task DeleteProduct(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product != null)
        {
            db.Products.Remove(product);
            await db.SaveChangesAsync();
        }
    }
}