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
}