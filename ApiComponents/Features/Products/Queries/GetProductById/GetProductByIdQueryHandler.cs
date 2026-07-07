using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using MediatR;


namespace ApiComponents.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponseDto?>
{
    private readonly IProductRepository _repo;

    public GetProductByIdQueryHandler(IProductRepository repo) => _repo = repo;

    public async Task<ProductResponseDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repo.GetProduct(request.Id, cancellationToken);
    }
}
