using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using MediatR;

namespace ApiComponents.Application.Features.Categories.Commands;

public record UpdateCategoryCommand(ProductCategory Category) : IRequest<bool>;

public class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository) : IRequestHandler<UpdateCategoryCommand, bool>
{
    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        await categoryRepository.UpdateCategoryAsync(request.Category, cancellationToken);
        return true;
    }
}
