using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using MediatR;


namespace ApiComponents.Application.Features.Brands.Commands;

public record UpdateBrandCommand(BrandRequestDTo Brand) : IRequest<BrandRequestDTo>;

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, BrandRequestDTo>
{
    private readonly IBrandRepository _repo;

    public UpdateBrandCommandHandler(IBrandRepository repo) => _repo = repo;

    public async Task<BrandRequestDTo> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        await _repo.UpdateBrandAsync(request.Brand, cancellationToken);
        return request.Brand;
    }
}
