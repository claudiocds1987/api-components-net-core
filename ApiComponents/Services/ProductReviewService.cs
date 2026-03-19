using ApiComponents.Models;
using ApiComponents.Persistence.Repositories;

namespace ApiComponents.Services
{
    public class ProductReviewService(IProductReviewRepository reviewRepo) : IProductReviewService
    {
        public async Task CreateReviewAsync(ProductReview review)
        {
            // Aquí podrías agregar validaciones extra antes de guardar
            if (review.rating < 1 || review.rating > 5)
                throw new ApplicationException("La puntuación debe estar entre 1 y 5.");

            await reviewRepo.AddReview(review);
        }

        public async Task<List<ProductReview>> GetReviewsByProductIdAsync(int productId)
        {
            return await reviewRepo.GetReviewsByProductId(productId);
        }

        public async Task DeleteReviewAsync(int id)
        {
            await reviewRepo.DeleteReview(id);
        }
    }
}