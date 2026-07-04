using MediatR;

namespace ApiComponents.Features.Products.Commands.UpdateProductStatus;

public record UpdateProductStatusCommand(int Id, bool IsActive) : IRequest<ApiComponents.DTOs.ProductRequestDTo>;
