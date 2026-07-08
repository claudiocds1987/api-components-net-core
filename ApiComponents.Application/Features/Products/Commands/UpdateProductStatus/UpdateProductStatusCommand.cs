using ApiComponents.Application.DTOs;
using MediatR;

namespace ApiComponents.Application.Features.Products.Commands.UpdateProductStatus;

public record UpdateProductStatusCommand(int Id, bool IsActive) : IRequest<ProductRequestDTo>;
