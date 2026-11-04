using ApiComponents.Application.DTOs;
using ApiComponents.Application.Features.ProductReviews.Commands;
using ApiComponents.Application.Features.ProductReviews.Queries;
using ApiComponents.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/reviews")]
[ApiController]
public class ReviewController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] ProductReview review)
    {
        try
        {
            await sender.Send(new CreateReviewCommand(review));
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

    // Retorna ProductReviewDto para no exponer UserEmail en endpoints públicos.
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<List<ProductReviewDto>>> GetProductReviews(int productId)
    {
        var reviews = await sender.Send(new GetReviewsByProductIdQuery(productId));
        return Ok(reviews);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        try
        {
            await sender.Send(new DeleteReviewCommand(id));
            return Ok(new { message = "Review eliminada con éxito." });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "Error al eliminar la review.");
        }
    }
}
