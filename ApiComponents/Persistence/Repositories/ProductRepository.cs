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

    public async Task<Product> GetProduct(int id) => await db.Products.FindAsync(id);

    public async Task<(List<Product> Items, int TotalCount)> GetProductsAsync(int? page, int? size)
    {
        var query = db.Products
            .Include(p => p.images) // Incluimos relaciones con EntityFramework para obtener imagenes y tags
            .Include(p => p.tags)
            .AsQueryable();

        int totalCount = await query.CountAsync();

        if (page.HasValue && size.HasValue)
        {
            // - Skip(n): (EntityFramework) Para decirle a la base de datos cuántos registros debe saltar desde el principio de la lista.
            // Ej: Si estoy en la página 3 y cada página tiene 10 productos, debe saltar los primeros 20.
            // - Take(n): (EntityFramework) Para decirle a la base de datos cuántos registros debe tomar a partir de donde terminó el salto. Es el tamaño de tu página.
            query = query.Skip((page.Value - 1) * size.Value).Take(size.Value);
        }

        var items = await query.ToListAsync();
        return (items, totalCount);
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