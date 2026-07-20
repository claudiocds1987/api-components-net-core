using MediatR;
using ApiComponents.Application.Repositories;

namespace ApiComponents.Application.Features.Products.Queries.GetProductsAdmin;

// endpoint liviano para la Grilla Admin
public record GetProductsAdminQuery(int? Page, int? Size, string? Search, int? CategoryId, int? BrandId,
    decimal? MinPrice, decimal? MaxPrice, string? SortBy, string? Order, bool? IsActive) : IRequest<object>;


    public class GetProductsAdminQueryHandler : IRequestHandler<GetProductsAdminQuery, object>
    {
        private readonly IProductRepository _repo;

        public GetProductsAdminQueryHandler(IProductRepository repo)
        {
            _repo = repo;
        }

        public async Task<object> Handle(GetProductsAdminQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetProductsAdminAsync(
                request.Page, request.Size, request.Search,
                request.CategoryId, request.BrandId, request.MinPrice,
                request.MaxPrice, request.SortBy, request.Order,
                request.IsActive, cancellationToken);

            return new { items, totalItems = total };
        }
    }


