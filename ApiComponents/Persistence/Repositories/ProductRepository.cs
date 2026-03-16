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