using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Brands.Queries;

public record GetBrandByIdQuery(int Id) : IRequest<BrandResponseDTo?>;

public class GetBrandByIdQueryHandler(IBrandRepository brandRepository) : IRequestHandler<GetBrandByIdQuery, BrandResponseDTo?>
{
    public async Task<BrandResponseDTo?> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetBrandByIdAsync(request.Id, cancellationToken);
        if (brand == null) return null;

        return new BrandResponseDTo
        {
            id = brand.id,
            name = brand.name,
            isActive = brand.isActive
        };
    }
}