using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.Brands.Commands;

public record CreateBrandCommand(BrandRequestDTo Brand) : IRequest<Unit>;

public class CreateBrandCommandHandler(IBrandRepository repo) : IRequestHandler<CreateBrandCommand, Unit>
{
    public async Task<Unit> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        // Validación de existencia de marca
        if (await repo.ExistBrandAsync(request.Brand.name, cancellationToken))
            throw new ApplicationException("La marca ya existe.");

        var newBrandEntity = new ProductBrand
        {
            name = request.Brand.name,
            isActive = request.Brand.isActive
        };

        await repo.CreateBrandAsync(newBrandEntity, cancellationToken);
        return Unit.Value;
    }
}