using MediatR;
using System.Collections;

namespace ApiComponents.Features.Products.Queries.GetProducts;

public record GetProductsQuery(int? Page, int? Size, string? Search, int? CategoryId, int? BrandId,
    decimal? MinPrice, decimal? MaxPrice, string? SortBy, string? Order, bool? IsActive, bool? ForAdmin = null) : IRequest<object>;
