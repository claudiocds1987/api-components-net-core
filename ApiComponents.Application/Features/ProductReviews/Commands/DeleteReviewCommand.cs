using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.ProductReviews.Commands;

public record DeleteReviewCommand(int Id) : IRequest<Unit>;

public class DeleteReviewCommandHandler(IProductReviewRepository repo) : IRequestHandler<DeleteReviewCommand, Unit>
{
    public async Task<Unit> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        // DeleteReview elimina la review y recalcula rating/reviewCount del producto
        // en una única transacción atómica dentro del repositorio.
        await repo.DeleteReview(request.Id);
        return Unit.Value;
    }
}
