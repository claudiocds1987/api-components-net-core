using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Persistence.Repositories;

public class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<bool> ExistCategory(string name)
        => await context.ProductCategories.AnyAsync(c => c.name.ToLower() == name.ToLower());

    public async Task<IEnumerable<ProductCategory>> GetAllCategories(bool? isActive = true)
    {
        var query = context.ProductCategories.AsQueryable();
        // Filtro inteligente: si es null trae todo, si tiene valor filtra
        if (isActive.HasValue)
            query = query.Where(c => c.isActive == isActive.Value);

        return await query.ToListAsync();
    }

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
            // SOFT DELETE: Cambiamos estado de lapropiedad isActive a false, en lugar de eliminar el registro de la base de datos
            c.isActive = false;
            context.Entry(c).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }
    }
}