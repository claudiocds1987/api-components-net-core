using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Brands.Commands;

public record DeleteBrandCommand(int Id) : IRequest<Unit>;

public class DeleteBrandCommandHandler(IBrandRepository brandRepository) : IRequestHandler<DeleteBrandCommand, Unit>
{
    public async Task<Unit> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetBrandByIdAsync(request.Id, cancellationToken);
        if (brand == null)
            throw new ApplicationException($"La marca con ID {request.Id} no existe.");

        await brandRepository.DeleteBrandAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}