using MediatR;

namespace ApiComponents.Application.Features.Products.Queries.GetProducts;
// endpoint Listado de productos para Customer 
public record GetProductsQuery(int? Page, int? Size, string? Search, int? CategoryId, int? BrandId,
    decimal? MinPrice, decimal? MaxPrice, string? SortBy, string? Order, bool? IsActive) : IRequest<object>;
