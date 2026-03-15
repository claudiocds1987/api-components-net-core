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
        => Ok(await brandService.GetAllBrandsAsync());

    [HttpPost]
    public async Task<IActionResult> CreateBrand(ProductBrand brand)
    {
        try
        {
            await brandService.CreateBrandAsync(brand);
            return Ok(brand);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        await brandService.DeleteBrandAsync(id);
        return NoContent();
    }
}