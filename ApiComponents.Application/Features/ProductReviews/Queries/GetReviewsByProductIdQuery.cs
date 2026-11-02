using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.ProductReviews.Queries;

public record GetReviewsByProductIdQuery(int ProductId) : IRequest<List<ProductReview>>;

public class GetReviewsByProductIdQueryHandler(IProductReviewRepository repo)
    : IRequestHandler<GetReviewsByProductIdQuery, List<ProductReview>>
{
    public async Task<List<ProductReview>> Handle(GetReviewsByProductIdQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetReviewsByProductId(request.ProductId);
    }
}
