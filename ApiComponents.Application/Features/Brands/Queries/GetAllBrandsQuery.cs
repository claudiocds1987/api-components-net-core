using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Brands.Queries;

public record GetAllBrandsQuery(bool? IsActive = true) : IRequest<IEnumerable<BrandResponseDTo>>;

public class GetAllBrandsQueryHandler(IBrandRepository brandRepository) : IRequestHandler<GetAllBrandsQuery, IEnumerable<BrandResponseDTo>>
{
    public async Task<IEnumerable<BrandResponseDTo>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
    {
        return await brandRepository.GetAllBrandsAsync(request.IsActive, cancellationToken);
    }
}
