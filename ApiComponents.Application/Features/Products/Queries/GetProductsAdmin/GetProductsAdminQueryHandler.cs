using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Products.Queries.GetProductsAdmin
{
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
}