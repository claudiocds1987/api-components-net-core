using ApiComponents.Application.DTOs;
using ApiComponents.Application.Features.Brands.Commands;
using ApiComponents.Application.Features.Brands.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BrandController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBrands([FromQuery] bool? isActive = true)
        => Ok(await sender.Send(new GetAllBrandsQuery(isActive)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrand(int id)
    {
        var brand = await sender.Send(new GetBrandByIdQuery(id));
        return brand is not null ? Ok(brand) : NotFound($"La marca con ID {id} no existe.");
    }

    [HttpPost]
    public async Task<IActionResult> CreateBrand([FromBody] BrandRequestDTo brand)
    {
        await sender.Send(new CreateBrandCommand(brand));
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateBrand([FromBody] BrandRequestDTo brand)
    {
        await sender.Send(new UpdateBrandCommand(brand));
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        await sender.Send(new DeleteBrandCommand(id));
        return NoContent();
    }
}