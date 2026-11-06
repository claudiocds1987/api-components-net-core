using MediatR;
using AutoMapper;
using ApiComponents.Application.Repositories;
using ApiComponents.Application.DTOs;

namespace ApiComponents.Application.Features.Products.Queries;

// endpoint Listado de productos para Customer 
public record GetProductsQuery(
    int? Page,
    int? Size,
    string? Search,
    List<int>? CategoryIds,
    List<int>? BrandIds,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? MinRating,
    string? SortBy,
    string? Order,
    bool? IsActive) : IRequest<object>;


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
        // Por defecto en e-commerce: ordenar por rating de mayor a menor (mejores valorados primero)
        string sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "rating" : request.SortBy;
        string order = string.IsNullOrWhiteSpace(request.Order)
            ? (sortBy.Equals("rating", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc")
            : request.Order;

        var (items, total) = await _repo.GetProductsAsync(
            request.Page, request.Size, request.Search,
            request.CategoryIds, request.BrandIds,
            request.MinPrice, request.MaxPrice,
            sortBy, order,
            request.IsActive, request.MinRating, cancellationToken);

        var dtos = _mapper.Map<List<ProductDto>>(items);
        return new { items = dtos, totalItems = total };
    }
}
