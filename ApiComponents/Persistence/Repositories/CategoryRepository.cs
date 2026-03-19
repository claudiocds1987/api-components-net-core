using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Persistence.Repositories;

public class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<bool> ExistCategory(string name)
        => await context.ProductCategories.AnyAsync(c => c.name.ToLower() == name.ToLower());

    public async Task<IEnumerable<ProductCategory>> GetAllCategories()
        => await context.ProductCategories.ToListAsync();

    public async Task<ProductCategory?> GetCategory(int id)
        => await context.ProductCategories.FindAsync(id);

    public async Task AddCategory(ProductCategory category)
    {
        await context.ProductCategories.AddAsync(category);
        await context.SaveChangesAsync();
    }

    public async Task UpdateCategory(ProductCategory category)
    {
        context.Entry(category).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteCategory(int id)
    {
        var c = await GetCategory(id);
        if (c != null)
        {
            context.ProductCategories.Remove(c);
            await context.SaveChangesAsync();
        }
    }
}