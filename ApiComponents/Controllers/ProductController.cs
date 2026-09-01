using ApiComponents.Application.DTOs;
using ApiComponents.Domain.Models;
using MediatR;

using Microsoft.AspNetCore.Mvc;
using ApiComponents.Application.Features.Products.Queries;
using ApiComponents.Application.Features.Products.Commands;

namespace ApiComponents.Controllers;

[Route("api/products")]
[ApiController]
public class ProductController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query)
    {
        return Ok(await sender.Send(query));
    }

    [HttpGet("admin")]
    public async Task<IActionResult> GetProductsAdmin([FromQuery] GetProductsAdminQuery query)
    {
        return Ok(await sender.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await sender.Send(new GetProductByIdQuery(id));

        if (product == null)
            return NotFound(new { message = $"El producto con ID {id} no fue encontrado." });

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequestDTo product)
    {
        if (product == null) return BadRequest(new { message = "El producto no puede ser nulo." });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var command = new CreateProductCommand(product, Request.Scheme, Request.Host.Value);
        return Ok(new { message = "Producto creado con éxito", data = await sender.Send(command) });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequestDTo productDto)
    {
        if (id != productDto.id) return BadRequest(new { message = "El ID no coincide." });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updatedProduct = await sender.Send(new UpdateProductCommand(productDto, Request.Scheme, Request.Host.Value));
        return Ok(new { message = "Producto actualizado correctamente", data = updatedProduct });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateProductStatus(int id, [FromQuery] bool isActive)
    {
        var updatedProduct = await sender.Send(new UpdateProductStatusCommand(id, isActive));
        return Ok(new { message = $"Producto {(isActive ? "activado" : "desactivado")} correctamente", data = updatedProduct });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var updatedProduct = await sender.Send(new UpdateProductStatusCommand(id, false));
        return Ok(new { message = "Producto dado de baja correctamente", data = updatedProduct });
    }
}