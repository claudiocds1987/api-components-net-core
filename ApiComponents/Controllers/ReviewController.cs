using ApiComponents.Domain.Models;
using ApiComponents.Services; // Cambiamos la referencia al servicio
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/reviews")]
[ApiController]
public class ReviewController(IProductReviewService reviewService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateReview(ProductReview review)
    {
        try
        {
            await reviewService.CreateReviewAsync(review);
            return Ok(new { message = "Review publicada con éxito." });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "Error al publicar la review.");
        }
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<List<ProductReview>>> GetProductReviews(int productId)
    {
        var reviews = await reviewService.GetReviewsByProductIdAsync(productId);
        return Ok(reviews);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        await reviewService.DeleteReviewAsync(id);
        return NoContent();
    }
}