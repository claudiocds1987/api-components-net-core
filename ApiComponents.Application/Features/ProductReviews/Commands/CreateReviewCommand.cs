using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.ProductReviews.Commands;

public record CreateReviewCommand(ProductReview Review) : IRequest<Unit>;

public class CreateReviewCommandHandler(IProductReviewRepository repo) : IRequestHandler<CreateReviewCommand, Unit>
{
    public async Task<Unit> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        if (request.Review.rating < 1 || request.Review.rating > 5)
            throw new ApplicationException("La puntuación debe estar entre 1 y 5.");

        // AddReview crea la review y recalcula rating/reviewCount del producto
        // en una única transacción atómica dentro del repositorio.
        await repo.AddReview(request.Review);

        return Unit.Value;
    }
}
