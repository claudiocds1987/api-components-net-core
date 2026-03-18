using ApiComponents.Models;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/products")]
[ApiController]
public class ProductController(IProductService productService) : ControllerBase
{

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.id) return BadRequest("El ID del producto no coincide");

        try
        {
            await productService.UpdateProductAsync(product);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await productService.GetProductByIdAsync(id);
        return product == null ? NotFound() : Ok(product);
    }

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await productService.DeleteProductAsync(id);
        return NoContent();
    }
}