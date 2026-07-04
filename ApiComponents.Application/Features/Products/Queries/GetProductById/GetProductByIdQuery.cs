using MediatR;
using ApiComponents.Application.DTOs;

namespace ApiComponents.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<ProductResponseDto?>;
