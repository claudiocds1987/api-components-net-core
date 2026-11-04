using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using ApiComponents.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Infrastructure.Repositories;

public class ProductReviewRepository(AppDbContext db) : IProductReviewRepository
{
    /// <summary>
    /// Crea la review y recalcula rating/reviewCount del producto en una única transacción
    /// ejecutando un UNICO SaveChangesAsync que persiste tanto la review como el producto en un solo batch.
    /// </summary>
    public async Task AddReview(ProductReview review)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 1. Consulta agregada única de las reviews existentes antes de insertar la nueva
                var existingStats = await db.ProductReviews
                    .Where(r => r.productId == review.productId)
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        SumRating = g.Sum(r => (decimal)r.rating),
                        Count = g.Count()
                    })
                    .FirstOrDefaultAsync();

                // 2. Cálculo exacto del nuevo promedio y conteo incluyendo la nueva review
                int totalCount = (existingStats?.Count ?? 0) + 1;
                decimal totalSum = (existingStats?.SumRating ?? 0m) + review.rating;
                decimal newAverage = Math.Round(totalSum / totalCount, 2);

                // 3. Registrar la nueva review en el ChangeTracker
                review.date = DateTime.UtcNow;
                await db.ProductReviews.AddAsync(review);

                // 4. Obtener y actualizar el producto en el ChangeTracker
                var product = await db.Products.FindAsync(review.productId)
                    ?? throw new KeyNotFoundException($"Producto con ID {review.productId} no encontrado.");

                product.UpdateStatistics(newAverage, totalCount);

                // 5. UNICO SaveChangesAsync para toda la operación (INSERT + UPDATE en un solo batch)
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    /// <summary>
    /// Proyecta directamente a ProductReviewDto para garantizar que UserEmail
    /// nunca llegue a la capa de presentación (seguridad por diseño).
    /// </summary>
    public async Task<List<ProductReviewDto>> GetReviewsByProductId(int productId)
    {
        return await db.ProductReviews
            .Where(r => r.productId == productId)
            .OrderByDescending(r => r.date) // Reviews más nuevas primero
            .Select(r => new ProductReviewDto
            {
                id       = r.id,
                rating   = r.rating,
                comment  = r.comment,
                userName = r.userName,
                date     = r.date
                // userEmail se omite intencionalmente
            })
            .ToListAsync();
    }

    /// <summary>
    /// Elimina la review y recalcula rating/reviewCount del producto en una única transacción
    /// ejecutando un UNICO SaveChangesAsync que persiste tanto la eliminación como la actualización en un solo batch.
    /// </summary>
    public async Task DeleteReview(int id)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var review = await db.ProductReviews.FindAsync(id)
                    ?? throw new KeyNotFoundException($"Review con ID {id} no encontrada.");

                int productId = review.productId;

                // 1. Consulta agregada de las reviews restantes (excluyendo la que se va a eliminar)
                var remainingStats = await db.ProductReviews
                    .Where(r => r.productId == productId && r.id != id)
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        SumRating = g.Sum(r => (decimal)r.rating),
                        Count = g.Count()
                    })
                    .FirstOrDefaultAsync();

                // 2. Cálculo exacto del nuevo promedio y conteo sin la review eliminada
                int newCount = remainingStats?.Count ?? 0;
                decimal newAverage = newCount > 0
                    ? Math.Round(remainingStats!.SumRating / newCount, 2)
                    : 0m;

                // 3. Marcar la review para eliminación en el ChangeTracker
                db.ProductReviews.Remove(review);

                // 4. Obtener y actualizar el producto en el ChangeTracker
                var product = await db.Products.FindAsync(productId)
                    ?? throw new KeyNotFoundException($"Producto con ID {productId} no encontrado.");

                product.UpdateStatistics(newAverage, newCount);

                // 5. UNICO SaveChangesAsync para toda la operación (DELETE + UPDATE en un solo batch)
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}
