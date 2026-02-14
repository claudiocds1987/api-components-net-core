using ApiComponents.Persistence.Repositories;
using ApiComponents.DTOs;
using System.Text.Json;
using System.Net.Http.Json;

namespace ApiComponents.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly IGeminiRepository _aiRepo;
        private readonly HttpClient _httpClient;

        public GeminiService(IGeminiRepository aiRepo, HttpClient httpClient)
        {
            _aiRepo = aiRepo;
            _httpClient = httpClient;
        }

        public async Task<string> ConsultarConCatalogoAsync(string preguntaUsuario)
        {
            try
            {
                // 1. Análisis de búsqueda
                var intencionJson = await AnalyzeSearchAsync(preguntaUsuario);

                // Validación extra: Si el análisis ya falló por cuota, detenemos aquí
                if (intencionJson.Contains("RESOURCE_EXHAUSTED") || intencionJson.Contains("TooManyRequests"))
                    throw new Exception("RESOURCE_EXHAUSTED");

                string terminoBusqueda = "";
                try
                {
                    var jsonLimpio = intencionJson.Replace("```json", "").Replace("```", "").Trim();
                    using var doc = JsonDocument.Parse(jsonLimpio);
                    terminoBusqueda = doc.RootElement.GetProperty("busqueda").GetString() ?? "";
                }
                catch { terminoBusqueda = preguntaUsuario.Split(' ').Last().Replace("?", ""); }

                // 2. DummyJSON
                var dummyUrl = $"https://dummyjson.com/products/search?q={Uri.EscapeDataString(terminoBusqueda)}";
                var searchResult = await _httpClient.GetFromJsonAsync<DummyProductResponseDto>(dummyUrl);

                // 3. Contexto
                string contextoCatalogo = searchResult?.Products?.Any() == true
                    ? string.Join("\n", searchResult.Products.Take(2).Select(p => $"- {p.Title} (${p.Price})"))
                    : "Sin stock.";

                // 4. Prompt y llamada final
                var promptMaestro = $"Actúa como vendedor. Contexto: {contextoCatalogo}. Pregunta: {preguntaUsuario}";

                var respuestaFinal = await _aiRepo.GenerateTextAsync(promptMaestro);

                // Si la respuesta final trae el error de cuota, lanzamos excepción
                if (respuestaFinal.Contains("RESOURCE_EXHAUSTED") || respuestaFinal.Contains("429"))
                    throw new Exception("RESOURCE_EXHAUSTED");

                return respuestaFinal;
            }
            catch (Exception ex)
            {
                // Log del error real para el desarrollador (tú)
                Console.WriteLine($"DEBUG ERROR: {ex.Message}");

                // Respuesta amigable para el cliente (Angular)
                if (ex.Message.Contains("RESOURCE_EXHAUSTED") || ex.Message.Contains("429"))
                {
                    return "¡Hola! Estoy recibiendo muchas consultas en este momento. Por favor, espera unos 20 segundos para que pueda procesar tu solicitud correctamente. ¡Gracias!";
                }

                return "Lo siento, hubo un problema al consultar el catálogo. ¿Podrías intentar de nuevo?";
            }
        }

        public async Task<string> AnalyzeSearchAsync(string text)
        {
            var prompt = $@"Analiza la pregunta del usuario y extrae exclusivamente el NOMBRE del producto o marca que busca.
    Usuario: '{text}'
    Responde SOLO un JSON: {{""busqueda"": ""valor""}}
    Ejemplo: 'tienes algo de essence?' -> {{""busqueda"": ""essence""}}";

            return await _aiRepo.GenerateTextAsync(prompt);
        }

        public async Task<string> GetVendedorAnswerAsync(GeminiProductRequestDto request)
        {
            var productData = JsonSerializer.Serialize(request.Context);
            var prompt = $"Producto: {request.Title}. Datos: {productData}. Pregunta: {request.Question}. " +
                         "Responde de forma vendedora en 2 frases máximo.";
            return await _aiRepo.GenerateTextAsync(prompt);
        }
    }
}