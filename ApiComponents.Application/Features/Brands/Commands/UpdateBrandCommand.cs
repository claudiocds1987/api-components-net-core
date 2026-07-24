using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Brands.Commands;

public record UpdateBrandCommand(BrandRequestDTo Brand) : IRequest<Unit>;

public class UpdateBrandCommandHandler(IBrandRepository repo) : IRequestHandler<UpdateBrandCommand, Unit>
{
    public async Task<Unit> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brandEntity = await repo.GetBrandByIdAsync(request.Brand.id, cancellationToken);
        if (brandEntity == null)
            throw new ApplicationException("La marca no existe.");

        brandEntity.name = request.Brand.name;
        brandEntity.isActive = request.Brand.isActive;

        await repo.UpdateBrandAsync(brandEntity, cancellationToken);
        return Unit.Value;
    }
}