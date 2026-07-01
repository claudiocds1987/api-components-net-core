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

    [HttpPost("save-extra-attributes/{categoryId}")] // Endpoint para guardar o actualizar atributos extra de una categoría
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveExtraAttributes(int categoryId, [FromBody] List<ProductExtraAttributesDto> attributes)
    {
        if (attributes == null || categoryId <= 0)
        {
            return BadRequest(new { message = "Datos de entrada inválidos." });
        }

        try
        {
            // Ejecuta la lógica de sincronización (Insert/Update)
            await attributeService.SaveExtraAttributes(categoryId, attributes);

            // Retorna un 200 OK con el mensaje de confirmación
            return Ok(new { message = "Atributos guardados exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Ocurrió un error al intentar guardar los atributos.",
                detail = ex.Message
            });
        }
    }

    [HttpDelete("{extraAttributeId}")]
    public async Task<IActionResult> Delete(int extraAttributeId)
    {
        try
        {
            await attributeService.DeleteExtraAttributeAsync(extraAttributeId);
            return Ok(new { message = "Atributo y sus valores asociados eliminados correctamente." });
        }
        catch (System.Collections.Generic.KeyNotFoundException) { return NotFound(); }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }
}