using MediatR;
using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Application.Features.Products.Queries
{

    public record GetTotalProductsByBrandQuery(bool? IsActive = true) : IRequest<IEnumerable<TotalProductsByBrandDto>>;

    public class GetTotalProductsByBrandQueryHandler(IProductRepository repo)
    : IRequestHandler<GetTotalProductsByBrandQuery, IEnumerable<TotalProductsByBrandDto>>
    {
        public async Task<IEnumerable<TotalProductsByBrandDto>> Handle(GetTotalProductsByBrandQuery request, CancellationToken cancellationToken)
        {
            return await repo.GetTotalProductsByBrandAsync(request.IsActive, cancellationToken);
        }
    }

}
