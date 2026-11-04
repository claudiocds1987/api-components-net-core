using ApiComponents.Application.DTOs;
using ApiComponents.Domain.Models;

namespace ApiComponents.Application.Repositories;

public interface IProductReviewRepository
{
    /// <summary>
    /// Crea una review y recalcula las estadísticas del producto en la misma transacción.
    /// </summary>
    Task AddReview(ProductReview review);

    Task<List<ProductReviewDto>> GetReviewsByProductId(int productId);

    /// <summary>
    /// Elimina una review y recalcula las estadísticas del producto en la misma transacción.
    /// </summary>
    Task DeleteReview(int id);
}
