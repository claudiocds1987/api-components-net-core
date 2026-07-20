using MediatR;
using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(ProductRequestDTo Product, string Scheme, string Host) : IRequest<Unit>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Unit>
{
    private readonly IProductRepository _repo;

    public CreateProductCommandHandler(IProductRepository repo) => _repo = repo;

    public async Task<Unit> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validaciones de negocio mÃ­nimas delegadas a la entidad en Domain (si aplica)
        await _repo.CreateProduct(request.Product, request.Scheme, request.Host, cancellationToken);
        return Unit.Value;
    }
}

