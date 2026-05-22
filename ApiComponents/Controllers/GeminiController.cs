using Microsoft.AspNetCore.Mvc;
using ApiComponents.Services;
using ApiComponents.DTOs;

namespace ApiComponents.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public GeminiController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        // Este es el endpoint que usa el ia Chat en Angular
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] GeminiSimpleRequestDto request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Prompt))
                return BadRequest("The question cannot be empty.");

            var response = await _geminiService.QueryCatalogAsync(request.Prompt, cancellationToken);
            return Ok(response);
        }

        // Mantenemos este por si quieres usarlo para análisis de texto puro
        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] Dictionary<string, string> body)
        {
            if (!body.ContainsKey("text")) return BadRequest("Falta el campo 'text'");
            var response = await _geminiService.AnalyzeSearchAsync(body["text"]);
            return Ok(response);
        }

        // endpoint específico para cuando el usuario ya está viendo un producto puntual
        [HttpPost("seller-expert")]
        public async Task<IActionResult> SellerExpert([FromBody] GeminiProductRequestDto request, CancellationToken cancellationToken)
        {
            var response = await _geminiService.GetSellerAnswerAsync(request, cancellationToken);
            return Ok(new { response });
        }
    }

    // DTO simple para el chat general
    public class GeminiSimpleRequestDto
    {
        public string Prompt { get; set; } = string.Empty;
    }
}