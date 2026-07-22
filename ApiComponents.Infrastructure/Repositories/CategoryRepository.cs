using ApiComponents.Domain.Models;
using ApiComponents.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Infrastructure.Repositories
{

    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistCategoryAsync(string name, CancellationToken cancellationToken = default)
            => await _context.ProductCategories.AnyAsync(c => c.name.ToLower() == name.ToLower(), cancellationToken);

        public async Task<IEnumerable<ProductCategory>> GetAllAsync(bool? isActive = true, CancellationToken cancellationToken = default)
        {
            var query = _context.ProductCategories.AsQueryable();
            // Filtro inteligente: si es null trae todo, si tiene valor filtra
            if (isActive.HasValue)
                query = query.Where(c => c.isActive == isActive.Value);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<ProductCategory?> GetCategoryAsync(int id, CancellationToken cancellationToken = default)
            => await _context.ProductCategories.FindAsync(id, cancellationToken);

        public async Task AddCategoryAsync(ProductCategory category, CancellationToken cancellationToken = default)
        {
            await _context.ProductCategories.AddAsync(category, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateCategoryAsync(ProductCategory category, CancellationToken cancellationToken = default)
        {
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            var c = await GetCategoryAsync(id, cancellationToken);
            if (c != null)
            {
                // SOFT DELETE: Cambiamos estado de la propiedad isActive a false, en lugar de eliminar el registro de la base de datos
                c.isActive = false;
                _context.Entry(c).State = EntityState.Modified;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}