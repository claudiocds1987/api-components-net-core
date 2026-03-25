using ApiComponents.Models;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductCategory>>> GetCategories([FromQuery] bool? isActive = true)
    {
        return Ok(await categoryService.GetAllCategoriesAsync(isActive));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductCategory>> GetCategory(int id)
    {
        var category = await categoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound($"La categoría con ID {id} no existe.");

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(ProductCategory category)
    {
        try
        {
            await categoryService.CreateCategoryAsync(category);
            // Uso de CreatedAtAction para devolver 201 y la ubicación del recurso
            return CreatedAtAction(nameof(GetCategory), new { id = category.id }, category);
        }
        catch (ApplicationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            await categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}