using MediatR;
using ApiComponents.DTOs;

namespace ApiComponents.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(ProductRequestDTo Product, string Scheme, string Host) : IRequest<Unit>;
