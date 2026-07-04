using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Services;
using MediatR;
using ApiComponents.Features.Products.Commands.CreateProduct;
using ApiComponents.Features.Products.Commands.UpdateProduct;
using ApiComponents.Features.Products.Commands.UpdateProductStatus;
using ApiComponents.Features.Products.Queries.GetProducts;
using ApiComponents.Features.Products.Queries.GetProductById;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/products")]
[ApiController]
public class ProductController(ISender sender) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetProducts(
      [FromQuery] int? page = null,
      [FromQuery] int? size = null,
      [FromQuery] string? search = null,
      [FromQuery] int? categoryId = null,
      [FromQuery] int? brandId = null,
      [FromQuery] decimal? minPrice = null,
      [FromQuery] decimal? maxPrice = null,
      [FromQuery] string? sortBy = "rating", // Por defecto ordenamos por rating, pero el cliente puede elegir otro campo
      [FromQuery] string? order = "asc", // Por defecto ordenamos de forma ascendente
      [FromQuery] bool? isActive = true)
    {
        try
        {
            var query = new GetProductsQuery(page, size, search, categoryId, brandId, minPrice, maxPrice, sortBy, order, isActive);
            var result = await sender.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("admin")]
    public async Task<IActionResult> GetProductsAdmin(
     [FromQuery] int? page = null,
     [FromQuery] int? size = null,
     [FromQuery] string? search = null,
     [FromQuery] int? categoryId = null,
     [FromQuery] int? brandId = null,
     [FromQuery] decimal? minPrice = null,
     [FromQuery] decimal? maxPrice = null,
     [FromQuery] string? sortBy = "id",
     [FromQuery] string? order = "desc",
     [FromQuery] bool? isActive = null) // Null para que el admin vea "Todos" al entrar activos e inactivos por defecto
    {
        try
        {
            var query = new GetProductsQuery(page, size, search, categoryId, brandId, minPrice, maxPrice, sortBy, order, isActive, true);
            var result = await sender.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await sender.Send(new GetProductByIdQuery(id));

        // Al usar el Product? en el servicio, esto elimina cualquier warning 
        // y asegura que Angular reciba un 404 real si el ID no existe.
        if (product == null)
        {
            return NotFound(new { message = $"El producto con ID {id} no fue encontrado." });
        }

        return Ok(product);
    }


    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequestDTo product)
    {
        if (product == null) return BadRequest(new { message = "El producto no puede ser nulo." });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var command = new CreateProductCommand(product, Request.Scheme, Request.Host.Value);
            await sender.Send(command);
            return Ok(new { message = "Producto creado con éxito", data = product });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error crítico al procesar el producto",
                details = ex.Message,
                stack = ex.StackTrace,
                inner = ex.InnerException?.Message
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequestDTo productDto)
    {
        if (id != productDto.id) return BadRequest(new { message = "El ID no coincide." });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            // Capturamos el DTO con los IDs e imágenes reales procesadas
            var updatedProduct = await sender.Send(new UpdateProductCommand(productDto, Request.Scheme, Request.Host.Value));

            // Devolvemos ese objeto Product actualizado en la propiedad 'data'
            return Ok(new { message = "Producto actualizado correctamente", data = updatedProduct });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar el producto", details = ex.Message });
        }
    }

    // ENDPOINT PARA DAR DE BAJA/ALTA de un producto (pasa isActive a false o true)
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateProductStatus(int id, [FromQuery] bool isActive)
    {
        try
        {
            var updatedProduct = await sender.Send(new UpdateProductStatusCommand(id, isActive));
            return Ok(new { message = $"Producto {(isActive ? "activado" : "desactivado")} correctamente", data = updatedProduct });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al cambiar el estado del producto", details = ex.Message });
        }
    }

    // ENDPOINT PARA DAR DE BAJA (Cuando el frontend hace un HTTP DELETE, si bien se puede usar UpdateProductStatus directamente por protocolo hice DeleteProduct)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var updatedProduct = await sender.Send(new UpdateProductStatusCommand(id, false));
            return Ok(new { message = "Producto dado de baja correctamente", data = updatedProduct });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al dar de baja el producto", details = ex.Message });
        }
    }

}