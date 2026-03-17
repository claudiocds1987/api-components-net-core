using ApiComponents.Models;

namespace ApiComponents.Persistence.Repositories;

public interface IProductReviewRepository
{
    Task AddReview(ProductReview review);
    Task<List<ProductReview>> GetReviewsByProductId(int productId);
    Task DeleteReview(int id);
}