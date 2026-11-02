using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using ApiComponents.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Infrastructure.Repositories;

public class ProductReviewRepository(AppDbContext db) : IProductReviewRepository
{
    public async Task AddReview(ProductReview review)
    {
        // Seteamos la fecha actual al crearla
        review.date = DateTime.UtcNow;
        await db.ProductReviews.AddAsync(review);
        await db.SaveChangesAsync();
    }

    public async Task<List<ProductReview>> GetReviewsByProductId(int productId)
    {
        return await db.ProductReviews
            .Where(r => r.productId == productId)
            .OrderByDescending(r => r.date) // Las reviews más nuevas primero
            .ToListAsync();
    }

    public async Task DeleteReview(int id)
    {
        var review = await db.ProductReviews.FindAsync(id);
        if (review != null)
        {
            db.ProductReviews.Remove(review);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<TotalProductsByRatingDto>> GetTotalProductsByRatingAsync(CancellationToken cancellationToken = default)
    {
        // 1. Obtenemos el promedio de rating de cada producto de forma individual
        var productAverages = db.ProductReviews
            .AsNoTracking()
            .GroupBy(r => r.productId)
            .Select(g => new
            {
                ProductId = g.Key,
                // Redondeamos el promedio al entero más cercano (1 a 5)
                AverageRating = (int)Math.Round(g.Average(r => (double)r.rating))
            });

        // 2. Agrupamos esos promedios para contar cuántos productos hay por cada estrella
        return await productAverages
            .GroupBy(x => x.AverageRating)
            .Select(g => new TotalProductsByRatingDto
            {
                Rating = g.Key,
                TotalProducts = g.Count()
            })
            .OrderByDescending(x => x.Rating)
            .ToListAsync(cancellationToken);
    }
}