using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/massiveImport")]
public class ImportController(IProductService productService) : ControllerBase
{
    [HttpPost("products")]
    public async Task<IActionResult> ImportProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Seleccioná un archivo CSV válido." });

        // Llamamos al proceso del servicio
        var result = await productService.ProcessCsvAsync(file);

        if (!result.Success)
        {
            // Devolvemos 400 con la lista de errores para que Angular los muestre
            return BadRequest(result);
        }

        return Ok(result);
    }
}