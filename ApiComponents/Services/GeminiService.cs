using ApiComponents.Persistence.Repositories;
using ApiComponents.DTOs;
using System.Text.Json;
using System.Net.Http.Json;

namespace ApiComponents.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly IGeminiRepository _aiRepo;
        private readonly IProductRepository _productRepo;
        // private readonly HttpClient _httpClient;

        public GeminiService(IGeminiRepository aiRepo, IProductRepository productRepo)
        {
            _aiRepo = aiRepo;
            _productRepo = productRepo;
            //_httpClient = httpClient;
        }

        public async Task<GeminiChatResponseDto> QueryCatalogAsync(string userQuestion)
        {
            // 1. Carga de productos desde la base de datos
            var dbResult = await _productRepo.GetProductsAsync(page: 1, size: 200, isActive: true);

            var allProducts = dbResult.Items.Select(p => new DummyProductDto
            {
                id = p.id,
                title = p.title,
                description = p.description,
                price = (decimal)p.price,
                discountPercentage = (double)p.discountPercentage,
                rating = (double)p.rating,
                stock = p.stock,
                category = p.category?.name ?? "General",
                brand = p.brand?.name ?? "N/A",
                thumbnail = p.thumbnail,
                tags = p.tags?.Select(t => t.tagName).ToList() ?? new List<string>()
            }).ToList();

            // LOG 1: Verificar si están llegando productos de la DB
            Console.WriteLine($"DEBUG: [Fase 1] Productos cargados desde DB: {allProducts.Count}");

            // 2. IA: Clasificar la intención del usuario
            string intentCategory = await GetIntentCategoryFromAI(userQuestion);

            // LOG 2: Ver qué categoría decidió la IA
            Console.WriteLine($"DEBUG: [Fase 2] La IA determinó la categoría: '{intentCategory}'");

            // Función auxiliar de normalización
            string NormalizeText(string s) => s?.ToLower()
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Trim() ?? "";

            // 3. Tokenización de la pregunta del usuario
            var keywords = NormalizeText(userQuestion)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2)
                .ToList();

            Console.WriteLine($"DEBUG: [Fase 3] Keywords extraídas: {string.Join(", ", keywords)}");

            List<DummyProductDto> products = new();

            // 4. PASO A: Intentar por Categoría (Comparación robusta)
            if (intentCategory != "none" && intentCategory != "OFFERS")
            {
                products = allProducts
                    .Where(p => string.Equals(NormalizeText(p.category), NormalizeText(intentCategory), StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Console.WriteLine($"DEBUG: [Fase 4] Productos encontrados en categoría '{intentCategory}': {products.Count}");

                // Refinamiento por palabras clave dentro de la categoría
                if (products.Any())
                {
                    var refined = products.Where(p =>
                        keywords.Any(k => NormalizeText(p.title).Contains(k)) ||
                        keywords.Any(k => NormalizeText(p.description).Contains(k))
                    ).ToList();

                    if (refined.Any())
                    {
                        products = refined;
                        Console.WriteLine($"DEBUG: [Fase 4b] Refinamiento exitoso dentro de categoría. Items: {products.Count}");
                    }
                }
            }

            // 5. PASO B: Respaldo (Si la categoría falló o dio 0, buscamos en TODO el catálogo)
            if (!products.Any())
            {
                Console.WriteLine("DEBUG: [Fase 5] No hubo resultados por categoría. Iniciando búsqueda general por texto libre...");

                products = allProducts.Where(p =>
                    keywords.Any(k => NormalizeText(p.title).Contains(k)) ||
                    keywords.Any(k => NormalizeText(p.description).Contains(k)) ||
                    keywords.Any(k => NormalizeText(p.category).Contains(k))
                ).ToList();

                Console.WriteLine($"DEBUG: [Fase 5] Resultados búsqueda general: {products.Count}");
            }

            // 6. RESPUESTA FINAL
            Console.WriteLine($"DEBUG: [Final] Enviando al front {products.Count} productos.");

            if (products.Any())
            {
                return new GeminiChatResponseDto
                {
                    Response = $"¡Claro! He encontrado {products.Count} opciones de '{userQuestion}' para ti:",
                    Products = products
                };
            }

            return new GeminiChatResponseDto
            {
                Response = $"No encontré resultados exactos para '{userQuestion}'. ¿Te gustaría intentar con palabras más simples?",
                Products = new List<DummyProductDto>()
            };
        }

        private async Task<string> GetIntentCategoryFromAI(string userText)
        {
            try
            {
                var prompt = $@"Eres un clasificador de categorías para una tienda.
        TU SALIDA DEBE SER ÚNICAMENTE UNA DE ESTAS CATEGORÍAS:
        laptops, smartphones, fragrances, skin-care, groceries, home-decoration, 
        furniture, tops, womens-dresses, womens-shoes, mens-shirts, mens-shoes, 
        mens-watches, womens-watches, womens-bags, womens-jewellery, sunglasses, 
        mobile-accessories, sports-accessories, motorcycle

        REGLAS:
        - Si el usuario pide algo de mujer y es un reloj, responde: womens-watches
        - Si pide algo de hombre y es un reloj, responde: mens-watches
        - Si busca ofertas, responde: OFFERS
        - Si no es nada de la lista, responde: none

        Usuario dice: '{userText}'
        Respuesta:";

                var response = await _aiRepo.GenerateTextAsync(prompt);
                return response.Trim().ToLower();
            }
            catch { return "none"; }
        }
        //private async Task<string> GetIntentCategoryFromAI(string userText)
        //{
        //    try
        //    {
        //        var prompt =
        //            $@"Actúa como un clasificador semántico para un catálogo de productos.

        //    CATEGORÍAS DISPONIBLES EN EL SISTEMA:
        //    - Ropa: 'mens-shirts', 'womens-dresses', 'tops'
        //    - Calzado: 'mens-shoes', 'womens-shoes'
        //    - Relojes: 'mens-watches', 'womens-watches'
        //    - Accesorios: 'womens-bags', 'womens-jewellery', 'sunglasses'
        //    - Tecnología: 'laptops', 'smartphones', 'tablets', 'mobile-accessories'
        //    - Hogar/Otros: 'fragrances', 'beauty', 'skin-care', 'furniture', 'groceries', 'home-decoration', 'kitchen-accessories'

        //    REGLAS CRÍTICAS:
        //    1. Si el usuario pregunta de forma GENERAL (ej: 'relojes', 'calzado', 'zapatos', 'ropa'), responde ÚNICAMENTE la palabra raíz en inglés: 'watches', 'shoes', 'shirts' o 'dresses'.
        //    2. Si el usuario especifica GÉNERO (ej: 'relojes de mujer', 'zapatos para hombre'), responde la categoría completa: 'womens-watches' o 'mens-shoes'.
        //    3. Si el usuario busca ofertas, descuentos o promociones, responde: 'OFFERS'.
        //    4. Si no detectas ninguna categoría del catálogo, responde: 'NONE'.

        //    EJEMPLOS:
        //    - '¿tienen relojes?' -> watches
        //    - 'relojes de mujer' -> womens-watches
        //    - 'zapatillas' -> shoes
        //    - 'tenis de hombre' -> mens-shoes
        //    - 'perfumes' -> fragrances

        //    Entrada del usuario: '{userText}'
        //    Respuesta (SOLO la palabra clave):";

        //        var response = await _aiRepo.GenerateTextAsync(prompt);
        //        return response.Trim().ToLower();
        //    }
        //    catch (Exception ex)
        //    {
        //        // En caso de saturación de exceder el límite de prompts, Gemini devuelve (503) o error de red, registramos el error y devolvemos "none"
        //        Console.WriteLine($"[IA LOG]: Error al clasificar intención (Gemini 503/Saturado): {ex.Message}");
        //        return "none";
        //    }
        //}

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
}


