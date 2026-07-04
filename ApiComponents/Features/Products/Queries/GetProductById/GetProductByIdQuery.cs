using MediatR;
using ApiComponents.DTOs;

namespace ApiComponents.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<ProductResponseDto?>;
