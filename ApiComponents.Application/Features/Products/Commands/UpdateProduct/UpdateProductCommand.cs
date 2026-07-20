using MediatR;
using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(ProductRequestDTo Product, string Scheme, string Host) : IRequest<ProductRequestDTo>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductRequestDTo>
{
    private readonly IProductRepository _repo;

    public UpdateProductCommandHandler(IProductRepository repo) => _repo = repo;

    public async Task<ProductRequestDTo> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        return await _repo.UpdateProduct(request.Product, request.Scheme, request.Host, cancellationToken);
    }
}

