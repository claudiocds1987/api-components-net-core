using MediatR;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ApiComponents.Application.DTOs.ProductResponseDto?>
{
    private readonly IProductRepository _repo;

    public GetProductByIdQueryHandler(IProductRepository repo) => _repo = repo;

    public async Task<ApiComponents.Application.DTOs.ProductResponseDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repo.GetProduct(request.Id, cancellationToken);
    }
}
