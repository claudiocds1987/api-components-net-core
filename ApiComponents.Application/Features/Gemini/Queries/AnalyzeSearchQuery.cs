using ApiComponents.Application.Repositories;
using MediatR;

namespace ApiComponents.Application.Features.Gemini.Queries;

/// <summary>
/// Extrae el objeto de búsqueda de un texto en lenguaje natural usando IA.
/// 
/// Esta función es para cuando el usuario ya tiene el producto/os en el componente ia-chat del frontend 
/// y quiere hacer una pregunta de un producto de esa lista.
/// 
/// PARÁMETROS:
/// - Text: El texto en lenguaje natural del que se debe extraer el objeto de búsqueda.
/// 
/// QUÉ HACE:
/// 1. Construye un prompt que instruye a la IA a extraer el objeto de búsqueda del texto proporcionado.
/// 2. La IA debe responder solo con JSON en formato: {"busqueda": "valor"}.
/// 3. Delega la generación de texto al repositorio de IA (Gemini).
/// 
/// DEVUELVE (return):
/// Un string con la respuesta JSON generada por la IA con el objeto de búsqueda extraído.
/// </summary>
public record AnalyzeSearchQuery(string Text) : IRequest<string>;

public class AnalyzeSearchQueryHandler(IGeminiRepository aiRepo) : IRequestHandler<AnalyzeSearchQuery, string>
{
    public async Task<string> Handle(AnalyzeSearchQuery request, CancellationToken cancellationToken)
    {
        var prompt = $@"Extrae el objeto de búsqueda de: '{request.Text}'. Responde solo JSON: {{""busqueda"": ""valor""}}";
        return await aiRepo.GenerateTextAsync(prompt, cancellationToken);
    }
}
