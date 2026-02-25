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

        public async Task<GeminiChatResponseDto> QueryCatalogAsync(string userQuestion)
        {
            // 1. Descargar catálogo completo (Aumentamos a 200 para capturar IDs altos como relojes de mujer)
            var allProductsUrl = "https://dummyjson.com/products?limit=200";
            var allProductsResult = await _httpClient.GetFromJsonAsync<DummyProductResponseDto>(allProductsUrl);
            var allProducts = allProductsResult?.Products ?? new List<DummyProductDto>();

            // 2. IA: Clasificar la intención del usuario
            string intentCategory = await GetIntentCategoryFromAI(userQuestion);

            List<DummyProductDto> products = new();
            string responseMsg = "¡Si, claro! Aquí están los productos:";

            // Función de normalización para búsquedas por título exacto
            string Normalize(string s) =>
                s.ToLower()
                 .Replace("-", "").Replace(" ", "")
                 .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                 .Replace("?", "").Replace(".", "").Replace(",", "");

            var normalizedPrompt = Normalize(userQuestion);

            // 3. Lógica de Filtrado Basada en la Intención
            if (intentCategory == "OFFERS")
            {
                products = allProducts.OrderByDescending(p => p.discountPercentage).Take(10).ToList();
                responseMsg = "¡Claro! Aquí tienes nuestras mejores ofertas del día:";
            }
            else if (intentCategory != "NONE")
            {
                // FILTRO MEJORADO: Usamos StringComparison para ser más robustos.
                // Si intentCategory es "watches", traerá "mens-watches" y "womens-watches".
                products = allProducts
                    .Where(p => p.category.Contains(intentCategory, StringComparison.OrdinalIgnoreCase) ||
                                (p.tags != null && p.tags.Any(t => t.Contains(intentCategory, StringComparison.OrdinalIgnoreCase))))
                    .ToList();

                responseMsg = $"¡Si, claro! Aquí tienes las opciones para {userQuestion}:";
            }
            else
            {
                // Búsqueda de respaldo por texto si la IA no detectó una categoría clara
                products = allProducts.Where(p =>
                    Normalize(p.title).Contains(normalizedPrompt) ||
                    p.description.ToLower().Contains(userQuestion.ToLower()) ||
                    p.brand.ToLower().Contains(userQuestion.ToLower())
                ).ToList();
            }

            // 4. Post-procesamiento: Añadir etiquetas (Badges) de Stock y Rating
            foreach (var prod in products)
            {
                var meta = "";
                if (prod.stock < 5) meta += " ⚠️ Last units!";
                if (prod.rating > 4.5) meta += " ⭐ Customer favorite";
                if (!string.IsNullOrWhiteSpace(meta))
                    prod.description = prod.description.Trim() + " " + meta;
            }

            // 5. Respuesta Final
            if (products.Any())
            {
                return new GeminiChatResponseDto
                {
                    Response = responseMsg,
                    Products = products
                };
            }

            return new GeminiChatResponseDto
            {
                Response = $"No encontré resultados exactos para '{userQuestion}'. ¿Te gustaría intentar con otra búsqueda?",
                Products = new List<DummyProductDto>()
            };
        }

        private async Task<string> GetIntentCategoryFromAI(string userText)
        {
            var prompt =
                $@"Actúa como un clasificador semántico para un catálogo de productos.
    
                CATEGORÍAS DISPONIBLES EN EL SISTEMA:
                - Ropa: 'mens-shirts', 'womens-dresses', 'tops'
                - Calzado: 'mens-shoes', 'womens-shoes'
                - Relojes: 'mens-watches', 'womens-watches'
                - Accesorios: 'womens-bags', 'womens-jewellery', 'sunglasses'
                - Tecnología: 'laptops', 'smartphones', 'tablets', 'mobile-accessories'
                - Hogar/Otros: 'fragrances', 'beauty', 'skin-care', 'furniture', 'groceries', 'home-decoration', 'kitchen-accessories'

                REGLAS CRÍTICAS:
                1. Si el usuario pregunta de forma GENERAL (ej: 'relojes', 'calzado', 'zapatos', 'ropa'), responde ÚNICAMENTE la palabra raíz en inglés: 'watches', 'shoes', 'shirts' o 'dresses'.
                2. Si el usuario especifica GÉNERO (ej: 'relojes de mujer', 'zapatos para hombre'), responde la categoría completa: 'womens-watches' o 'mens-shoes'.
                3. Si el usuario busca ofertas, descuentos o promociones, responde: 'OFFERS'.
                4. Si no detectas ninguna categoría del catálogo, responde: 'NONE'.

                EJEMPLOS:
                - '¿tienen relojes?' -> watches
                - 'relojes de mujer' -> womens-watches
                - 'zapatillas' -> shoes
                - 'tenis de hombre' -> mens-shoes
                - 'perfumes' -> fragrances

                Entrada del usuario: '{userText}'
                Respuesta (SOLO la palabra clave):";

            var response = await _aiRepo.GenerateTextAsync(prompt);
            return response.Trim().ToLower();
        }

        public async Task<string> GetSellerAnswerAsync(GeminiProductRequestDto request)
        {
            try
            {
                var productUrl = $"https://dummyjson.com/products/{request.ProductId}";
                var productData = await _httpClient.GetStringAsync(productUrl);

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


