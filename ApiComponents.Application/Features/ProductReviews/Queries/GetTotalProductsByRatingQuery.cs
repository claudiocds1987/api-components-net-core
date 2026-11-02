using MediatR;
using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Application.Features.ProductReviews.Queries;

public record GetTotalProductsByRatingQuery(bool? IsActive = true) : IRequest<IEnumerable<TotalProductsByRatingDto>>;

public class GetTotalProductsByRatingQueryHandler(IProductReviewRepository repo)
    : IRequestHandler<GetTotalProductsByRatingQuery, IEnumerable<TotalProductsByRatingDto>>
{
    public async Task<IEnumerable<TotalProductsByRatingDto>> Handle(GetTotalProductsByRatingQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetTotalProductsByRatingAsync(cancellationToken);
    }
}
