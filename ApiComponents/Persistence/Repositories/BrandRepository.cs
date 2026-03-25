using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Persistence.Repositories;

public class BrandRepository(AppDbContext context) : IBrandRepository
{
    public async Task<bool> ExistBrand(string name)
        => await context.ProductBrands.AnyAsync(b => b.name.ToLower() == name.ToLower());

    public async Task<IEnumerable<ProductBrand>> GetAllBrands(bool? isActive = true)
    {
        var query = context.ProductBrands.AsQueryable();

        // Filtro inteligente: null trae todo, true/false filtra por estado
        if (isActive.HasValue)
            query = query.Where(b => b.isActive == isActive.Value);

        return await query.ToListAsync();
    }



    public async Task<ProductBrand?> GetBrand(int id)
        => await context.ProductBrands.FindAsync(id);

    public async Task AddBrand(ProductBrand brand)
    {
        await context.ProductBrands.AddAsync(brand);
        await context.SaveChangesAsync();
    }

    public async Task UpdateBrand(ProductBrand brand)
    {
        context.Entry(brand).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteBrand(int id)
    {
        var b = await GetBrand(id);
        if (b != null)
        {
            // SOFT DELETE: Cambiamos el estado isActive a false en lugar de eliminar el registro de la base de datos
            b.isActive = false;
            context.Entry(b).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }
    }
}