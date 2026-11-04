using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.ProductReviews.Queries;

public record GetReviewsByProductIdQuery(int ProductId) : IRequest<List<ProductReviewDto>>;

public class GetReviewsByProductIdQueryHandler(IProductReviewRepository repo)
    : IRequestHandler<GetReviewsByProductIdQuery, List<ProductReviewDto>>
{
    public async Task<List<ProductReviewDto>> Handle(GetReviewsByProductIdQuery request, CancellationToken cancellationToken)
    {
        // El repositorio proyecta directamente a ProductReviewDto,
        // garantizando que UserEmail nunca salga hacia la capa de presentación.
        return await repo.GetReviewsByProductId(request.ProductId);
    }
}
