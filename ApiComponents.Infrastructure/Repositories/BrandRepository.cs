using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using ApiComponents.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Infrastructure.Repositories;

public class BrandRepository(AppDbContext context) : IBrandRepository
{
    public async Task<bool> ExistBrandAsync(string name, CancellationToken cancellationToken = default)
        => await context.ProductBrands.AnyAsync(b => b.name.ToLower() == name.ToLower(), cancellationToken);

    public async Task<IEnumerable<ProductBrand>> GetAllBrandsAsync(bool? isActive = true, CancellationToken cancellationToken = default)
    {
        var query = context.ProductBrands.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(b => b.isActive == isActive.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ProductBrand?> GetBrandByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.ProductBrands.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task CreateBrandAsync(ProductBrand brand, CancellationToken cancellationToken = default)
    {
        await context.ProductBrands.AddAsync(brand, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateBrandAsync(ProductBrand brand, CancellationToken cancellationToken = default)
    {
        // Al estar rastreada desde GetBrandByIdAsync, SaveChanges detecta los cambios automáticamente
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteBrandAsync(int id, CancellationToken cancellationToken = default)
    {
        var brand = await GetBrandByIdAsync(id, cancellationToken);

        if (brand != null)
        {
            brand.isActive = false;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}