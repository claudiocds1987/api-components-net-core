using MediatR;
using ApiComponents.Application.DTOs;

namespace ApiComponents.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(ProductRequestDTo Product, string Scheme, string Host) : IRequest<Unit>;
