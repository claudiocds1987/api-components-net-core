using ApiComponents.Models;
using ApiComponents.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductCategory>>> GetCategories()
        => Ok(await categoryService.GetAllCategoriesAsync());

    [HttpPost]
    public async Task<IActionResult> CreateCategory(ProductCategory category)
    {
        try
        {
            await categoryService.CreateCategoryAsync(category);
            return Ok(category);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }
}