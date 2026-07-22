using ApiComponents.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ApiComponents.Application.Features.Categories.Queries;
using ApiComponents.Application.Features.Categories.Commands;

namespace ApiComponents.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductCategory>>> GetCategories([FromQuery] bool? isActive = true)
    {
        return Ok(await sender.Send(new GetAllCategoriesQuery(isActive)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductCategory>> GetCategory(int id)
    {
        var category = await sender.Send(new GetCategoryByIdQuery(id));
        if (category == null) return NotFound($"La categoría con ID {id} no existe.");

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(ProductCategory category)
    {
        try
        {
            var createdCategory = await sender.Send(new CreateCategoryCommand(category));
            return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.id }, createdCategory);
        }
        catch (ApplicationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCategory(ProductCategory category)
    {
        try
        {
            await sender.Send(new UpdateCategoryCommand(category));
            return NoContent();
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            await sender.Send(new DeleteCategoryCommand(id));
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}