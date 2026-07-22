using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.Categories.Queries;

public record GetCategoryByIdQuery(int Id) : IRequest<ProductCategory?>;

public class GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetCategoryByIdQuery, ProductCategory?>
{
    public async Task<ProductCategory?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        return await categoryRepository.GetCategoryAsync(request.Id, cancellationToken);
    }
}
