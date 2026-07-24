using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using ApiComponents.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Infrastructure.Repositories;

public class BrandRepository(AppDbContext context) : IBrandRepository
{
    public async Task<bool> ExistBrandAsync(string name, CancellationToken cancellationToken = default)
        => await context.ProductBrands.AnyAsync(b => b.name.ToLower() == name.ToLower(), cancellationToken);

    public async Task<IEnumerable<BrandResponseDTo>> GetAllBrandsAsync(bool? isActive = true, CancellationToken cancellationToken = default)
    {
        var query = context.ProductBrands.AsQueryable();

        // Filtro inteligente: null trae todo, true/false filtra por estado
        if (isActive.HasValue)
            query = query.Where(b => b.isActive == isActive.Value);

        return await query.Select(b => new BrandResponseDTo
        {
            id = b.id,
            name = b.name,
            isActive = b.isActive
        }).ToListAsync(cancellationToken);
    }



    public async Task<BrandResponseDTo?> GetBrandAsync(int id, CancellationToken cancellationToken = default)
    {
        var brand = await context.ProductBrands.FindAsync(new object[] { id }, cancellationToken);
        if (brand == null) return null;
        return new BrandResponseDTo
        {
            id = brand.id,
            name = brand.name,
            isActive = brand.isActive
        };
    }

    public async Task CreateBrandAsync(BrandRequestDTo brand, CancellationToken cancellationToken = default)
    {
        var productBrand = new ProductBrand
        {
            name = brand.name,
            isActive = brand.isActive
        };

        await context.ProductBrands.AddAsync(productBrand, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateBrandAsync(BrandRequestDTo brand, CancellationToken cancellationToken = default)
    {
        var productBrand = await GetBrandAsync(brand.id, cancellationToken);
        if (productBrand == null) throw new ApplicationException("La marca no existe.");

        productBrand.name = brand.name;
        productBrand.isActive = brand.isActive;

        context.Entry(productBrand).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteBrandAsync(int id, CancellationToken cancellationToken = default)
    {
        // Buscamos directamente la entidad en el DbSet
        var brand = await context.ProductBrands.FindAsync(new object[] { id }, cancellationToken);

        if (brand != null)
        {
            // Aplicamos el soft delete sobre la entidad rastreada
            brand.isActive = false;

            // Guardamos los cambios
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}