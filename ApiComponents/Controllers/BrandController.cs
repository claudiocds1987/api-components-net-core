using ApiComponents.Application.DTOs;
using ApiComponents.Application.Features.Brands.Commands;
using ApiComponents.Application.Features.Brands.Queries;
using ApiComponents.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BrandController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductBrand>>> GetBrands([FromQuery] bool? isActive = true)
    {
        var brands = await sender.Send(new GetAllBrandsQuery(isActive));
        return Ok(brands);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductBrand>> GetBrand(int id)
    {
        var brand = await sender.Send(new GetBrandByIdQuery(id));

        if (brand == null)
        {
            return NotFound($"La marca con ID {id} no existe.");
        }

        return Ok(brand);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBrand(BrandRequestDTo brand)
    {
        try
        {
            await sender.Send(new CreateBrandCommand(brand));
            return CreatedAtAction(nameof(GetBrand), new { id = brand.id }, brand);
        }
        catch (ApplicationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        try
        {
            await sender.Send(new DeleteBrandCommand(id));
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}