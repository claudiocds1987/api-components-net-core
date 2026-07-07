using MediatR;

using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductRequestDTo>
{
    private readonly IProductRepository _repo;

    public UpdateProductCommandHandler(IProductRepository repo) => _repo = repo;

    public async Task<ProductRequestDTo> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        return await _repo.UpdateProduct(request.Product, request.Scheme, request.Host, cancellationToken);
    }
}
