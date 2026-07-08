using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ApiComponents.Application.Repositories;
using Microsoft.Extensions.Configuration;

namespace ApiComponents.Infrastructure.Repositories
{
    public class GeminiRepository : IGeminiRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ModelName = "gemini-2.5-flash"; // si hay demanda probar "gemini-2.5-flash" gemini-3-flash-preview"

        public GeminiRepository(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("ApiKey no configurada");
        }

        public async Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent?key={_apiKey}";

            var requestBody = new GeminiRequest
            {
                Contents = new[] { new Content { Parts = new[] { new Part { Text = prompt } } } }
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

                // Si la API responde con error (como el 429 de cuota agotada)
                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();

                    // IMPORTANTE: Lanzamos excepción en lugar de retornar el string del error.
                    // Esto permite que el Service lo capture en su bloque catch.
                    throw new HttpRequestException($"GEMINI_ERROR|{(int)response.StatusCode}|{errorDetails}");
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);
                return result?.Candidates?[0].Content?.Parts?[0].Text ?? "No se recibió respuesta del modelo.";
            }
            catch (HttpRequestException)
            {
                // Re-lanzamos para que el servicio la maneje
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado en GeminiRepository: {ex.Message}");
            }
        }
    }

    // --- Clases de soporte para el mapeo del JSON (POCOs) ---
    public class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public Content[] Contents { get; set; } = null!;
    }

    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public Part[]? Parts { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}