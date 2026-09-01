using ApiComponents.Application.DTOs;
using MediatR;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Application.Features.Products.Commands;

public record UpdateProductStatusCommand(int Id, bool IsActive) : IRequest<ProductRequestDTo>;

public class UpdateProductStatusCommandHandler : IRequestHandler<UpdateProductStatusCommand, ProductRequestDTo>
{
    private readonly IProductRepository _repo;

    public UpdateProductStatusCommandHandler(IProductRepository repo) => _repo = repo;

    public async Task<ProductRequestDTo> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
    {
        return await _repo.UpdateProductStatus(request.Id, request.IsActive, cancellationToken);
    }
}

