using ApiComponents.Models;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/products")]
[ApiController]
public class ProductController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] int? page = null, [FromQuery] int? size = null)
    {
        try
        {
            var result = await productService.GetAllProductsAsync(page, size);
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
        var product = await productService.GetProductByIdAsync(id);

        // Al usar el Product? en el servicio, esto elimina cualquier warning 
        // y asegura que Angular reciba un 404 real si el ID no existe.
        if (product == null)
        {
            return NotFound(new { message = $"El producto con ID {id} no fue encontrado." });
        }

        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
    {
        // Validación básica de coherencia
        if (id != product.id)
            return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la petición.");

        try
        {
            await productService.UpdateProductAsync(product);
            return NoContent();
        }
        catch (Exception ex)
        {
            // Gracias al Fluent API, si intentas actualizar un producto con una CategoryId 
            // que no existe, saltará una excepción de base de datos que caerá aquí.
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            await productService.DeleteProductAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}