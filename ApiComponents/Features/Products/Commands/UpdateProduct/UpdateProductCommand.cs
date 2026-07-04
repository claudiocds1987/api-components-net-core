using MediatR;
using ApiComponents.DTOs;

namespace ApiComponents.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(ProductRequestDTo Product, string Scheme, string Host) : IRequest<ProductRequestDTo>;
