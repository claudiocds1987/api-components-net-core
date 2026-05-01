using ApiComponents.DTOs;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;


[ApiController]
[Route("api/ProductExtraAttributes")]
public class ProductExtraAttributesController(IProductAttributeService attributeService) : ControllerBase
{
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductExtraAttributesDto>>> GetByCategory(int categoryId)
    {
        try
        {
            var extraAttributes = await attributeService.GetAttributesByCategoryId(categoryId);

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
}