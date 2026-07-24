using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Brands.Commands;

public record DeleteBrandCommand(int Id) : IRequest<bool>;

public class DeleteBrandCommandHandler(IBrandRepository brandRepository) : IRequestHandler<DeleteBrandCommand, bool>
{
    public async Task<bool> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        await brandRepository.DeleteBrandAsync(request.Id, cancellationToken);
        return true;
    }
}
