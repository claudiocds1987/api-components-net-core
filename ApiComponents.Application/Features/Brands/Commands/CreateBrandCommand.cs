using ApiComponents.Application.Repositories;
using ApiComponents.Application.DTOs;
using MediatR;

namespace ApiComponents.Application.Features.Brands.Commands;

public record CreateBrandCommand(BrandRequestDTo Brand) : IRequest<Unit>;

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, Unit>
{
    private readonly IBrandRepository _repo;

    public CreateBrandCommandHandler(IBrandRepository repo) => _repo = repo;

    public async Task<Unit> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        // Validaciones de negocio mÃ­nimas delegadas a la entidad en Domain (si aplica)
        if (await _repo.ExistBrandAsync(request.Brand.name, cancellationToken))
            throw new ApplicationException("La marca ya existe.");

        await _repo.CreateBrandAsync(request.Brand, cancellationToken);
        return Unit.Value;
    }
}
