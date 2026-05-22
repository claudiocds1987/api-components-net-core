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
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(IGeminiRepository aiRepo, IProductRepository productRepo, ILogger<GeminiService> logger)
        {
            _aiRepo = aiRepo;
            _productRepo = productRepo;
            _logger = logger;
        }

        public async Task<GeminiChatResponseDto> QueryCatalogAsync(string userQuestion, CancellationToken cancellationToken = default)
        {
            // 1. Traemos los productos (solo los campos que Gemini necesita leer)
            var dbResult = await _productRepo.GetProductsAsync(page: 1, size: 250, isActive: true, cancellationToken: cancellationToken);

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
            var matchedIds = await GetSemanticMatchesFromAI(userQuestion, catalogJson, allProducts, cancellationToken);

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

        private async Task<List<ProductMatch>> GetSemanticMatchesFromAI(string userQuestion, string catalogJson, List<ProductDto> products, CancellationToken cancellationToken = default)
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
                var rawResponse = await _aiRepo.GenerateTextAsync(prompt, cancellationToken);

                // LOG: Detalle para debugging sin usar Console
                _logger.LogDebug("Raw AI response length: {Length}", rawResponse?.Length ?? 0);

                var cleanJson = rawResponse
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                _logger.LogDebug("Clean JSON preview: {Preview}", cleanJson.Length > 200 ? cleanJson[..200] : cleanJson);

                // Intent: ser tolerante con la forma en que Gemini devuelve el JSON.
                // Buscamos el array "matches" dentro del texto y lo parseamos manualmente.
                try
                {
                    var matches = new List<ProductMatch>();

                    var lower = cleanJson.ToLowerInvariant();
                    var matchesKey = "\"matches\"";
                    var idx = lower.IndexOf(matchesKey, StringComparison.Ordinal);

                    string arrayJson = null;

                    if (idx >= 0)
                    {
                        // Encontramos "matches", localizamos el '[' siguiente y el ']' que cierra
                        var arrayStart = cleanJson.IndexOf('[', idx);
                        if (arrayStart >= 0)
                        {
                            var arrayEnd = cleanJson.IndexOf(']', arrayStart);
                            if (arrayEnd > arrayStart)
                            {
                                arrayJson = cleanJson.Substring(arrayStart, arrayEnd - arrayStart + 1);
                            }
                        }
                    }

                    if (arrayJson == null)
                    {
                        // Como fallback tratamos de parsear todo el texto como JSON
                        arrayJson = cleanJson;
                    }

                    using var doc = JsonDocument.Parse(arrayJson);
                    // Si el doc es directamente un array
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            int id = 0;
                            double score = 0;

                            if (el.TryGetProperty("id", out var idProp))
                            {
                                if (idProp.ValueKind == JsonValueKind.Number && idProp.TryGetInt32(out var iid)) id = iid;
                                else if (idProp.ValueKind == JsonValueKind.String && int.TryParse(idProp.GetString(), out var pid)) id = pid;
                            }

                            if (el.TryGetProperty("score", out var scoreProp))
                            {
                                if (scoreProp.ValueKind == JsonValueKind.Number && scoreProp.TryGetDouble(out var s)) score = s;
                                else if (scoreProp.ValueKind == JsonValueKind.String && double.TryParse(scoreProp.GetString(), out var ps)) score = ps;
                            }

                            if (id != 0)
                            {
                                matches.Add(new ProductMatch { Id = id, Score = score });
                            }
                        }
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        if (doc.RootElement.TryGetProperty("matches", out var matchesEl) && matchesEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var el in matchesEl.EnumerateArray())
                            {
                                int id = 0;
                                double score = 0;

                                if (el.TryGetProperty("id", out var idProp))
                                {
                                    if (idProp.ValueKind == JsonValueKind.Number && idProp.TryGetInt32(out var iid)) id = iid;
                                    else if (idProp.ValueKind == JsonValueKind.String && int.TryParse(idProp.GetString(), out var pid)) id = pid;
                                }

                                if (el.TryGetProperty("score", out var scoreProp))
                                {
                                    if (scoreProp.ValueKind == JsonValueKind.Number && scoreProp.TryGetDouble(out var s)) score = s;
                                    else if (scoreProp.ValueKind == JsonValueKind.String && double.TryParse(scoreProp.GetString(), out var ps)) score = ps;
                                }

                                if (id != 0)
                                {
                                    matches.Add(new ProductMatch { Id = id, Score = score });
                                }
                            }
                        }
                    }

                    _logger.LogDebug("Matches count: {Count}", matches.Count);

                    // Si la IA no devolvió matches, aplicamos un fallback local simple: búsqueda por tokens
                    if (!matches.Any())
                    {
                        _logger.LogDebug("AI returned no matches, running local fallback matching.");

                        var fallback = new List<ProductMatch>();
                        var rawTokens = userQuestion.ToLowerInvariant().Split(new[] { ' ', ',', '.', '-' }, StringSplitOptions.RemoveEmptyEntries);

                        // Stopwords y mapeo de sinónimos básicos (español -> inglés y variantes)
                        var stopwords = new HashSet<string> { "de", "del", "la", "el", "los", "las", "para", "con", "y", "a" };

                        var synonyms = new Dictionary<string, string[]>
                        {
                            { "mujer", new[] { "woman", "women", "women's", "female", "femenino" } },
                            { "hombre", new[] { "man", "men", "male", "masculino" } },
                            { "reloj", new[] { "watch", "timepiece" } },
                            { "relojes", new[] { "watch", "watches" } },
                            { "plateado", new[] { "silver" } },
                            { "dorado", new[] { "gold", "golden" } },
                            { "negro", new[] { "black" } },
                            { "blanco", new[] { "white" } },
                            { "perfume", new[] { "fragrance", "perfume" } }
                        };

                        var tokens = new List<string>();
                        foreach (var t in rawTokens)
                        {
                            if (t.Length <= 2) continue; // ignorar tokens muy cortos
                            if (stopwords.Contains(t)) continue;
                            tokens.Add(t);
                            if (synonyms.TryGetValue(t, out var syns)) tokens.AddRange(syns);
                        }

                        foreach (var p in products)
                        {
                            var hay = (p.title + " " + p.brand + " " + p.category + " " + string.Join(" ", p.tags ?? new List<string>())).ToLowerInvariant();
                            var score = 0.0;
                            foreach (var t in tokens.Distinct())
                            {
                                if (hay.Contains(t)) score += 25; // cada token suma más peso
                            }
                            if (score >= 40) // aplicar mismo umbral que pedimos a la IA
                            {
                                fallback.Add(new ProductMatch { Id = p.id, Score = score });
                            }
                        }

                        // Ordenamos y retornamos max 20
                        return fallback.OrderByDescending(f => f.Score).Take(20).ToList();
                    }

                    return matches;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing 'matches' array from AI response. Returning empty list.");
                    return new List<ProductMatch>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializando respuesta de Gemini");
                return new List<ProductMatch>();
            }
        }

        public async Task<string> GetSellerAnswerAsync(GeminiProductRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _productRepo.GetProduct(request.ProductId, cancellationToken);

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

                return await _aiRepo.GenerateTextAsync(prompt, cancellationToken);
            }
            catch
            {
                return "No pude obtener los detalles en este momento. Inténtalo de nuevo.";
            }
        }

        public async Task<string> AnalyzeSearchAsync(string text, CancellationToken cancellationToken = default)
        {
            var prompt = $@"Extrae el objeto de búsqueda de: '{text}'. Responde solo JSON: {{""busqueda"": ""valor""}}";
            return await _aiRepo.GenerateTextAsync(prompt, cancellationToken);
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
        public double Score { get; set; }
    }
}


