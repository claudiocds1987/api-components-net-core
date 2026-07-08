using MediatR;

namespace ApiComponents.Application.Features.Products.Queries.GetProductsAdmin;
// endpoint liviano para la Grilla Admin
public record GetProductsAdminQuery(int? Page, int? Size, string? Search, int? CategoryId, int? BrandId,
    decimal? MinPrice, decimal? MaxPrice, string? SortBy, string? Order, bool? IsActive) : IRequest<object>;
