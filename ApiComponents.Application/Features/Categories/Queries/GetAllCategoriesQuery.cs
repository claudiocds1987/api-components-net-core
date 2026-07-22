using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.Categories.Queries;

public record GetAllCategoriesQuery(bool? IsActive = true) : IRequest<IEnumerable<ProductCategory>>;

public class GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetAllCategoriesQuery, IEnumerable<ProductCategory>>
{
    public async Task<IEnumerable<ProductCategory>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await categoryRepository.GetAllAsync(request.IsActive, cancellationToken);
    }
}
