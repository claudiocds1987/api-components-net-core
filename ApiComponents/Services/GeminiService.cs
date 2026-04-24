using ApiComponents.DTOs;
using ApiComponents.Persistence.Repositories;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiComponents.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly IGeminiRepository _aiRepo;
        private readonly IProductRepository _productRepo;

        public GeminiService(IGeminiRepository aiRepo, IProductRepository productRepo)
        {
            _aiRepo = aiRepo;
            _productRepo = productRepo;
        }

        public async Task<GeminiChatResponseDto> QueryCatalogAsync(string userQuestion)
        {
            // 1. Traemos los productos (solo los campos que Gemini necesita leer)
            var dbResult = await _productRepo.GetProductsAsync(page: 1, size: 250, isActive: true);

            var allProducts = dbResult.Items.Select(p => new ProductDto
            {
                id = p.id ?? 0,
                title = p.title,
                description = p.description,
                price = p.price,
                discountPercentage = (double)p.discountPercentage,
                rating = (double)p.rating,
                stock = p.stock,
                category = p.category?.name ?? "General",
                brand = p.brand?.name ?? "N/A",
                thumbnail = p.thumbnail,
                tags = p.tags?.Select(t => t.tagName).ToList() ?? new List<string>()
            }).ToList();

            // 2. Armamos un catálogo resumido para Gemini (sin description larga → ahorra tokens)
            var catalogSummary = allProducts.Select(p => new
            {
                p.id,
                p.title,
                p.brand,
                p.category,
                p.tags
            });

            var catalogJson = JsonSerializer.Serialize(catalogSummary);

            // 3. Gemini hace TODO el razonamiento semántico y devuelve JSON con IDs + scores
            var matchedIds = await GetSemanticMatchesFromAI(userQuestion, catalogJson);

            // 4. C# solo filtra por los IDs que Gemini eligió y respeta el orden por score
            var filteredProducts = matchedIds
                .OrderByDescending(m => m.Score)
                .Select(m => allProducts.FirstOrDefault(p => p.id == m.Id))
                .Where(p => p != null)
                .ToList();

            if (filteredProducts.Any())
            {
                return new GeminiChatResponseDto
                {
                    Response = $"¡Claro! Encontré {filteredProducts.Count} opciones para \"{userQuestion}\":",
                    Products = filteredProducts!
                };
            }

            return new GeminiChatResponseDto
            {
                Response = $"No encontré productos para \"{userQuestion}\". ¿Podés intentar con otros términos?",
                Products = new List<ProductDto>()
            };
        }

        private async Task<List<ProductMatch>> GetSemanticMatchesFromAI(string userQuestion, string catalogJson)
        {
            var prompt = $@"Sos un motor de búsqueda de productos. Tu tarea es encontrar los productos más relevantes para la consulta del usuario.

            CATÁLOGO (JSON):
            {catalogJson}

            CONSULTA DEL USUARIO: ""{userQuestion}""

            INSTRUCCIONES:
            - Analizá semánticamente la consulta: detectá categoría, género, color, material, estilo, uso, marca, precio aproximado, etc.
            - Traducí mentalmente los términos (""plateado"" = silver, ""reloj"" = watch, ""mujer"" = womens, ""perfume"" = fragrance, etc.)
            - Buscá coincidencias en title, brand, category y tags de cada producto.
            - Excluí productos del género opuesto si la consulta especifica género.
            - Asigná un score de 0 a 100 según relevancia.
            - Devolvé ÚNICAMENTE un JSON válido con este formato exacto, sin texto adicional:
            {{""matches"": [{{""id"": 1, ""score"": 95}}, {{""id"": 2, ""score"": 80}}]}}
            - Si no hay resultados relevantes devolvé: {{""matches"": []}}
            - Máximo 20 resultados, solo los de score >= 40.";

            try
            {
                var rawResponse = await _aiRepo.GenerateTextAsync(prompt);

                // TEMPORAL: loguea la respuesta cruda para ver qué devuelve Gemini
                Console.WriteLine("=== GEMINI RAW RESPONSE ===");
                Console.WriteLine(rawResponse);
                Console.WriteLine("===========================");

                var cleanJson = rawResponse
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                Console.WriteLine("=== CLEAN JSON ===");
                Console.WriteLine(cleanJson);
                Console.WriteLine("==================");

                var result = JsonSerializer.Deserialize<SemanticMatchResponse>(cleanJson);

                Console.WriteLine($"=== MATCHES COUNT: {result?.Matches?.Count ?? 0} ===");

                return result?.Matches ?? new List<ProductMatch>();
            }
            catch (Exception ex)
            {
                // Antes retornaba vacío sin que supieras por qué
                Console.WriteLine($"=== ERROR EN DESERIALIZE: {ex.Message} ===");
                return new List<ProductMatch>();
            }
        }

        public async Task<string> GetSellerAnswerAsync(GeminiProductRequestDto request)
        {
            try
            {
                var product = await _productRepo.GetProduct(request.ProductId);

                if (product == null) return "Lo siento, no pude encontrar los detalles de ese producto en nuestro catálogo actual.";

                // Serializamos tu modelo real para que la IA lo lea
                var productData = JsonSerializer.Serialize(product, new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                });

                var prompt = $@"Eres un experto vendedor. Responde dudas sobre este producto: {productData}.
                                Reglas: 
                                - Responde en el idioma del usuario.
                                - Sé breve (máximo 3 frases).
                                - Usa solo los datos proporcionados.
                                Pregunta: {request.UserMessage}";

                return await _aiRepo.GenerateTextAsync(prompt);
            }
            catch
            {
                return "No pude obtener los detalles en este momento. Inténtalo de nuevo.";
            }
        }

        public async Task<string> AnalyzeSearchAsync(string text)
        {
            var prompt = $@"Extrae el objeto de búsqueda de: '{text}'. Responde solo JSON: {{""busqueda"": ""valor""}}";
            return await _aiRepo.GenerateTextAsync(prompt);
        }
    }


    public class SemanticMatchResponse
    {
        [JsonPropertyName("matches")]
        public List<ProductMatch> Matches { get; set; } = new();
    }

    public class ProductMatch
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }
    }
}


