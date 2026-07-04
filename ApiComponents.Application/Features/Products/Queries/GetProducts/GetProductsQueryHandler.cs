using MediatR;
using ApiComponents.Application.Repositories;
using AutoMapper;

namespace ApiComponents.Application.Features.Products.Queries.GetProducts;

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
        if (request.ForAdmin.HasValue && request.ForAdmin.Value)
        {
            var (items, total) = await _repo.GetProductsAdminAsync(request.Page, request.Size, request.Search,
                request.CategoryId, request.BrandId, request.MinPrice, request.MaxPrice, request.SortBy, request.Order, request.IsActive, cancellationToken);

            return new { items, totalItems = total };
        }

        var (itemsPublic, totalPublic) = await _repo.GetProductsAsync(request.Page, request.Size, request.Search,
            request.CategoryId, request.BrandId, request.MinPrice, request.MaxPrice, request.SortBy, request.Order, request.IsActive, cancellationToken);

        var dtosPublic = _mapper.Map<List<ApiComponents.Application.DTOs.ProductDto>>(itemsPublic);
        return new { items = dtosPublic, totalItems = totalPublic };
    }
}
