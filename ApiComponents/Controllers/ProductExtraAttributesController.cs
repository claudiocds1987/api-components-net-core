using ApiComponents.DTOs;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;


[ApiController]
[Route("api/ProductExtraAttributes")]
public class ProductExtraAttributesController(IProductExtraAttributeService attributeService) : ControllerBase
{
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductExtraAttributesDto>>> GetByCategory(int categoryId)
    {
        try
        {
            var extraAttributes = await attributeService.GetExtraAttributesByCategoryId(categoryId);

            if (extraAttributes == null || !extraAttributes.Any())
                return Ok(new List<ProductExtraAttributesDto>());

            return Ok(extraAttributes);
        }
        catch (Exception ex)
        {
            // Documentación técnica: Captura errores de base de datos o mapeo
            return StatusCode(500, $"Error interno al recuperar atributos extra: {ex.Message}");
        }
    }

    [HttpPost("save-extra-attributes/{categoryId}")] // Endpoint para guardar y/o actualizar atributos extra de un producto
    public async Task<IActionResult> SaveExtraAttributes(int categoryId, [FromBody] List<ProductExtraAttributesDto> attributes)
    {
        if (attributes == null || categoryId <= 0)
            return BadRequest("Datos de entrada inválidos.");

        try
        {
            await attributeService.SaveExtraAttributes(categoryId, attributes);
            return Ok(new { message = "Atributos guardados correctamente" });
        }
        catch (Exception ex)
        {
            // Aquí podrías usar un logger para registrar el error real
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }
}