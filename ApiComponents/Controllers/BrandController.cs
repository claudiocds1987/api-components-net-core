using ApiComponents.Models;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BrandController(IBrandService brandService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductBrand>>> GetBrands()
    {
        var brands = await brandService.GetAllBrandsAsync();
        return Ok(brands);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductBrand>> GetBrand(int id)
    {
        var brand = await brandService.GetBrandByIdAsync(id);

        // Si el servicio devuelve null (porque pusimos el ProductBrand?), 
        // respondemos con un 404 claro.
        if (brand == null)
        {
            return NotFound($"La marca con ID {id} no existe.");
        }

        return Ok(brand);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBrand(ProductBrand brand)
    {
        try
        {
            await brandService.CreateBrandAsync(brand);

            // Convención REST: Devolver 201 (Created) y la ruta para obtener el recurso.
            return CreatedAtAction(nameof(GetBrand), new { id = brand.id }, brand);
        }
        catch (ApplicationException ex)
        {
            // Esto captura el error de "La marca ya existe" del Service
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        // Opcional: Podrías verificar si existe antes de borrar, 
        // pero tu Repo ya maneja el if (b != null).
        await brandService.DeleteBrandAsync(id);
        return NoContent();
    }
}