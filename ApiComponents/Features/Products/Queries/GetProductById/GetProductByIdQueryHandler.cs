using MediatR;
using ApiComponents.Persistence.Repositories;

namespace ApiComponents.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ApiComponents.DTOs.ProductResponseDto?>
{
    private readonly IProductRepository _repo;

    public GetProductByIdQueryHandler(IProductRepository repo) => _repo = repo;

    public async Task<ApiComponents.DTOs.ProductResponseDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repo.GetProduct(request.Id, cancellationToken);
    }
}
