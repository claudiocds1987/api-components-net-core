using ApiComponents.Domain.Models;

namespace ApiComponents.Services
{
    public interface IProductReviewService
    {
        Task CreateReviewAsync(ProductReview review);
        Task<List<ProductReview>> GetReviewsByProductIdAsync(int productId);
        Task DeleteReviewAsync(int id);
    }
}