using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Categories.Commands;

public record DeleteCategoryCommand(int Id) : IRequest<bool>;

public class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository) : IRequestHandler<DeleteCategoryCommand, bool>
{
    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        await categoryRepository.DeleteCategoryAsync(request.Id, cancellationToken);
        return true;
    }
}
