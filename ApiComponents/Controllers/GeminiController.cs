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
        public async Task<IActionResult> Ask([FromBody] GeminiSimpleRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Prompt))
                return BadRequest("La pregunta no puede estar vacía.");

            // Llamamos al nuevo método que integra DummyJSON + Gemini
            var response = await _geminiService.ConsultarConCatalogoAsync(request.Prompt);

            return Ok(response); // Ahora devuelve el objeto completo
        }

        // Mantenemos este por si quieres usarlo para análisis de texto puro
        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] Dictionary<string, string> body)
        {
            if (!body.ContainsKey("text")) return BadRequest("Falta el campo 'text'");
            var response = await _geminiService.AnalyzeSearchAsync(body["text"]);
            return Ok(response);
        }

        // Nuevo: Un endpoint específico para cuando el usuario ya está viendo 
        // un producto puntual
        [HttpPost("vendedor-experto")]
        public async Task<IActionResult> VendedorExperto([FromBody] GeminiProductRequestDto request)
        {
            var response = await _geminiService.GetVendedorAnswerAsync(request);
            return Ok(new { response });
        }
    }

    // DTO simple para el chat general
    public class GeminiSimpleRequestDto
    {
        public string Prompt { get; set; } = string.Empty;
    }
}