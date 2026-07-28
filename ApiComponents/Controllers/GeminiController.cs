using ApiComponents.Application.DTOs;
using ApiComponents.Application.Features.Gemini.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiComponents.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GeminiController(ISender sender) : ControllerBase
{
    // Este es el endpoint que usa el ia Chat en Angular
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] GeminiSimpleRequestDto request, CancellationToken cancellationToken)
        => Ok(await sender.Send(new QueryCatalogQuery(request.Prompt), cancellationToken));

    // Mantenemos este por si quieres usarlo para análisis de texto puro
    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] Dictionary<string, string> body, CancellationToken cancellationToken)
        => Ok(await sender.Send(new AnalyzeSearchQuery(body["text"]), cancellationToken));

    // endpoint específico para cuando el usuario ya está viendo un producto puntual
    [HttpPost("seller-expert")]
    public async Task<IActionResult> SellerExpert([FromBody] GeminiProductRequestDto request, CancellationToken cancellationToken)
        => Ok(new { response = await sender.Send(new GetSellerAnswerQuery(request.ProductId, request.UserMessage), cancellationToken) });
}

// DTO simple para el chat general
public class GeminiSimpleRequestDto
{
    public string Prompt { get; set; } = string.Empty;
}