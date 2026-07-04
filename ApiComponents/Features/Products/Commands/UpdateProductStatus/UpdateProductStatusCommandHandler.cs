using MediatR;
using ApiComponents.Persistence.Repositories;

namespace ApiComponents.Features.Products.Commands.UpdateProductStatus;

public class UpdateProductStatusCommandHandler : IRequestHandler<UpdateProductStatusCommand, ApiComponents.DTOs.ProductRequestDTo>
{
    private readonly IProductRepository _repo;

    public UpdateProductStatusCommandHandler(IProductRepository repo) => _repo = repo;

    public async Task<ApiComponents.DTOs.ProductRequestDTo> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
    {
        return await _repo.UpdateProductStatus(request.Id, request.IsActive, cancellationToken);
    }
}
