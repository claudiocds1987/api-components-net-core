using MediatR;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Unit>
{
    private readonly IProductRepository _repo;

    public CreateProductCommandHandler(IProductRepository repo) => _repo = repo;

    public async Task<Unit> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        await _repo.CreateProduct(request.Product, request.Scheme, request.Host, cancellationToken);
        return Unit.Value;
    }
}
