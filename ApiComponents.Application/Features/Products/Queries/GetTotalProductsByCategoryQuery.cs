using MediatR;
using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Application.Features.Products.Queries;

public record GetTotalProductsByCategoryQuery(bool? IsActive = true) : IRequest<IEnumerable<TotalProductsByCategoryDto>>;

public class GetTotalProductsByCategoryQueryHandler(IProductRepository repo)
    : IRequestHandler<GetTotalProductsByCategoryQuery, IEnumerable<TotalProductsByCategoryDto>>
{
    public async Task<IEnumerable<TotalProductsByCategoryDto>> Handle(GetTotalProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetTotalProductsByCategoryAsync(request.IsActive, cancellationToken);
    }
}
