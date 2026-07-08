using MediatR;
using ApiComponents.Application.DTOs;

namespace ApiComponents.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(ProductRequestDTo Product, string Scheme, string Host) : IRequest<ProductRequestDTo>;
