using MediatR;
using AutoMapper;
using ApiComponents.Application.Repositories;
using ApiComponents.Application.DTOs;

namespace ApiComponents.Application.Features.Products.Queries;

// endpoint Listado de productos para Customer 
public record GetProductsQuery(int? Page, int? Size, string? Search, int? CategoryId, int? BrandId,
    decimal? MinPrice, decimal? MaxPrice, string? SortBy, string? Order, bool? IsActive) : IRequest<object>;


public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, object>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<object> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _repo.GetProductsAsync(
            request.Page, request.Size, request.Search,
            request.CategoryId, request.BrandId, request.MinPrice,
            request.MaxPrice, request.SortBy, request.Order,
            request.IsActive, cancellationToken);

        var dtos = _mapper.Map<List<ProductDto>>(items);
        return new { items = dtos, totalItems = total };
    }
}


