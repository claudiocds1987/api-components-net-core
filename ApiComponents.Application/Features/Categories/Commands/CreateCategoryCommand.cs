using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.Categories.Commands;

public record CreateCategoryCommand(ProductCategory Category) : IRequest<ProductCategory>;

public class CreateCategoryCommandHandler(ICategoryRepository categoryRepository) : IRequestHandler<CreateCategoryCommand, ProductCategory>
{
    public async Task<ProductCategory> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await categoryRepository.ExistCategoryAsync(request.Category.name, cancellationToken))
            throw new ApplicationException("La categoría ya existe.");

        await categoryRepository.AddCategoryAsync(request.Category, cancellationToken);
        return request.Category;
    }
}
