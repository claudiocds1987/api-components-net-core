using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace ApiComponents.Persistence.Repositories
{
    public class GeminiRepository : IGeminiRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ModelName = "gemini-2.5-flash";

        public GeminiRepository(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("ApiKey no configurada");
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent?key={_apiKey}";

            var requestBody = new GeminiRequest
            {
                Contents = new[] { new Content { Parts = new[] { new Part { Text = prompt } } } }
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    return $"Error de API ({response.StatusCode}): {errorDetails}";
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                return result?.Candidates?[0].Content?.Parts?[0].Text ?? "No se recibió respuesta del modelo.";
            }
            catch (Exception ex)
            {
                return $"Error de red: {ex.Message}";
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