using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.ProductReviews.Commands;

public record DeleteReviewCommand(int Id) : IRequest<Unit>;

public class DeleteReviewCommandHandler(IProductReviewRepository repo) : IRequestHandler<DeleteReviewCommand, Unit>
{
    public async Task<Unit> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        await repo.DeleteReview(request.Id);
        return Unit.Value;
    }
}
