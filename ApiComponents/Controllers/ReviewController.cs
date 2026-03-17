using ApiComponents.Models;
using ApiComponents.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/reviews")]
[ApiController]
public class ReviewController(IProductReviewRepository reviewRepo) : ControllerBase
{
    // POST: api/reviews
    [HttpPost]
    public async Task<IActionResult> CreateReview(ProductReview review)
    {
        if (review == null) return BadRequest("Datos de review inválidos.");

        try
        {
            await reviewRepo.AddReview(review);
            return Ok(new { message = "Review publicada con éxito." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/reviews/product/5
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<List<ProductReview>>> GetProductReviews(int productId)
    {
        var reviews = await reviewRepo.GetReviewsByProductId(productId);
        return Ok(reviews);
    }

    // DELETE: api/reviews/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        await reviewRepo.DeleteReview(id);
        return NoContent();
    }
}