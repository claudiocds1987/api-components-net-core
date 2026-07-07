using MediatR;
using ApiComponents.Application.Repositories;
using ApiComponents.Application.DTOs;

namespace ApiComponents.Features.Products.Commands.UpdateProductStatus;

public class UpdateProductStatusCommandHandler : IRequestHandler<UpdateProductStatusCommand, ProductRequestDTo>
{
    private readonly IProductRepository _repo;

    public UpdateProductStatusCommandHandler(IProductRepository repo) => _repo = repo;

    public async Task<ProductRequestDTo> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
    {
        return await _repo.UpdateProductStatus(request.Id, request.IsActive, cancellationToken);
    }
}
