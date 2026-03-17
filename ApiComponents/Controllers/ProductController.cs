using ApiComponents.Models;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/products")]
[ApiController]
public class ProductController(IProductService productService) : ControllerBase
{
    [HttpPost("upload-excel")]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        // Validación básica de archivo
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Archivo no seleccionado o está vacío." });

        // Validar extensión del archivo
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".csv")
            return BadRequest(new { message = "Solo se permiten archivos .xlsx o .csv" });

        try
        {
            await productService.ProcessExcelAsync(file);
            return Ok(new { message = "Productos cargados exitosamente." });
        }
        catch (Exception ex)
        {
            // Aca 'ex.Message' contendrá toda la lista de errores (Fila 45, Fila 80, etc.)
            // que unimos con '\n' en el Service.
            return BadRequest(new
            {
                message = "Se encontraron errores en el archivo:",
                errors = ex.Message.Split('\n') // Lo enviamos como array para que Angular lo recorra fácil
            });
        }
    }

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