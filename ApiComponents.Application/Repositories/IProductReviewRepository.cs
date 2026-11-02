using ApiComponents.Application.DTOs;
using ApiComponents.Domain.Models;

namespace ApiComponents.Application.Repositories;

public interface IProductReviewRepository
{
    Task AddReview(ProductReview review);
    Task<List<ProductReview>> GetReviewsByProductId(int productId);

    Task<List<TotalProductsByRatingDto>> GetTotalProductsByRatingAsync(CancellationToken cancellationToken = default);
    Task DeleteReview(int id);
}