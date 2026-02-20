using ApiComponents.Persistence.Repositories;
using ApiComponents.DTOs;
using System.Text.Json;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

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
            // 1. Descargar todo el catálogo
            var allProductsUrl = "https://dummyjson.com/products?limit=100";
            var allProductsResult = await _httpClient.GetFromJsonAsync<DummyProductResponseDto>(allProductsUrl);
            var allProducts = allProductsResult?.Products ?? new List<DummyProductDto>();

            string lowerQuery = userQuestion.ToLower();
            var synonymsFragrances = new[] {
                "fragancia", "fragancias", "perfume", "perfumes", "aroma", "aromas",
                "oler bien", "buen aroma", "buen olor", "aromatizar", "fragante", "esencia"
            };
            var synonymsMakeup = new[] { "maquillaje", "makeup" };
            var synonymsSkincare = new[] { "skincare", "skin-care", "cuidado de la piel" };
            var synonymsOffers = new[] { "oferta", "ofertas", "descuento", "descuentos", "mejor precio" };

            var catalogCategories = new[]
            {
                "beauty", "fragrances", "furniture", "groceries", "home-decoration", "kitchen-accessories",
                "laptops", "mens-shirts", "mens-shoes", "mens-watches", "mobile-accessories", "motorcycle",
                "skin-care", "smartphones", "sports-accessories", "sunglasses", "tablets", "tops", "vehicle",
                "womens-bags", "womens-dresses", "womens-jewellery", "womens-shoes", "womens-watches"
            };

            List<DummyProductDto> products = new();
            string detectedCategory = "";
            bool isOffer = synonymsOffers.Any(s => lowerQuery.Contains(s));

            string Normalize(string s) =>
                s.ToLower()
                 .Replace("-", "")
                 .Replace(" ", "")
                 .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                 .Replace("?", "").Replace(".", "").Replace(",", "");

            var normalizedPrompt = Normalize(userQuestion);

            var productsByTitle = allProducts
                .Where(p => Normalize(p.Title) == normalizedPrompt)
                .ToList();

            if (productsByTitle.Any())
            {
                return new GeminiChatResponseDto
                {
                    Response = "Si, claro! Aquí esta el producto encontrado:",
                    Products = productsByTitle
                };
            }

            var productsByTag = allProducts
                .Where(p => p.Tags != null && p.Tags.Any(tag => normalizedPrompt.Contains(Normalize(tag)) || Normalize(tag).Contains(normalizedPrompt)))
                .ToList();

            if (productsByTag.Any())
            {
                return new GeminiChatResponseDto
                {
                    Response = "Si, claro! Aquí los productos relacionados a tu consulta:",
                    Products = productsByTag
                };
            }

            string categoryPrompt = catalogCategories.FirstOrDefault(cat =>
                normalizedPrompt.Contains(Normalize(cat)) ||
                Normalize(cat).Contains(normalizedPrompt) ||
                normalizedPrompt.Contains(Normalize(cat).TrimEnd('s')) ||
                normalizedPrompt.Contains(Normalize(cat) + "s")
            );

            if (!string.IsNullOrEmpty(categoryPrompt))
            {
                products = allProducts.Where(p => Normalize(p.Category) == Normalize(categoryPrompt)).ToList();
                detectedCategory = categoryPrompt.Replace("-", " ");
            }
            else if (synonymsFragrances.Any(s => lowerQuery.Contains(s)))
            {
                products = allProducts.Where(p => p.Category.ToLower() == "fragrances").ToList();
                detectedCategory = "fragancias";
            }
            else if (synonymsMakeup.Any(s => lowerQuery.Contains(s)))
            {
                products = allProducts.Where(p => p.Category.ToLower() == "beauty").ToList();
                detectedCategory = "maquillaje";
            }
            else if (synonymsSkincare.Any(s => lowerQuery.Contains(s)))
            {
                products = allProducts.Where(p => p.Category.ToLower().Contains("skin")).ToList();
                detectedCategory = "skincare";
            }
            else if (isOffer)
            {
                products = allProducts.OrderByDescending(p => p.DiscountPercentage).Take(10).ToList();
                detectedCategory = "ofertas";
            }
            else
            {
                products = allProducts.Where(p =>
                    lowerQuery.Split(' ').Any(q =>
                        p.Title.ToLower().Contains(q) ||
                        p.Description.ToLower().Contains(q) ||
                        p.Category.ToLower().Contains(q) ||
                        p.Brand.ToLower().Contains(q)
                    )
                ).ToList();
                detectedCategory = products.FirstOrDefault()?.Category ?? "producto";
            }

            foreach (var prod in products)
            {
                var meta = "";
                if (prod.Stock < 5) meta += " ⚠️ Last units!";
                if (prod.Rating > 4.5) meta += " ⭐ Customer favorite";
                if (!string.IsNullOrWhiteSpace(meta))
                    prod.Description = prod.Description.Trim() + meta;
            }

            if (products.Any())
            {
                string responseMsg = isOffer
                    ? "¡Claro! Aquí están nuestros productos con las mejores ofertas del día:"
                    : $"Si, claro! Aquí estan las opciones para {detectedCategory.ToLower()}:";
                if (isOffer)
                    products = products.OrderByDescending(p => p.DiscountPercentage).ToList();
                return new GeminiChatResponseDto
                {
                    Response = responseMsg,
                    Products = products
                };
            }
            else
            {
                string suggest = "fragancias";
                if (synonymsFragrances.Any(s => lowerQuery.Contains(s))) suggest = "maquillaje";
                else if (synonymsMakeup.Any(s => lowerQuery.Contains(s))) suggest = "skincare";
                else if (synonymsSkincare.Any(s => lowerQuery.Contains(s))) suggest = "fragancias";
                else if (isOffer) suggest = "fragancias";
                return new GeminiChatResponseDto
                {
                    Response = $"No se encontró resultado para {userQuestion} en el catalogo. Te gustaría ver otro producto?",
                    Products = new List<DummyProductDto>()
                };
            }
        }

        public async Task<string> AnalyzeSearchAsync(string text)
        {
            var prompt = $@"Analiza la pregunta del usuario y extrae exclusivamente el NOMBRE del producto o marca que busca.\n                         Usuario: '{text}'\n                         Responde SOLO un JSON: {{""busqueda"": ""valor""}}\n                         Ejemplo: 'tienes algo de essence?' -> {{""busqueda"": ""essence""}}";

            return await _aiRepo.GenerateTextAsync(prompt);
        }

        public async Task<string> GetSellerAnswerAsync(GeminiProductRequestDto request)
        {
            try
            {
                // 1. Consultamos el endpoint de producto específico en DummyJSON
                var productUrl = $"https://dummyjson.com/products/{request.ProductId}";
                var productData = await _httpClient.GetStringAsync(productUrl);

                // 2. Prompt Estratégico
                // pasamos el JSON entero a Gemini. Él sabe mapear "stock" a "existencias" 
                // o "dimensions" a "medidas" automáticamente.
                var prompt = $@"Eres un experto vendedor de ecommerce. 
                             Tu conocimiento actual es este producto en formato JSON: {productData}.
        
                            REGLAS DE RESPUESTA:
                                - Si el usuario pregunta en español, responde en español. If English, answer in English.
                                - Responde únicamente basado en los datos del JSON proporcionado.
                                - Si preguntan por 'características', haz un resumen breve de: descripción, dimensiones, garantía y políticas de devolución.
                                - Sé profesional, amable y no inventes datos que no estén en el JSON.
                                - Máximo 3 oraciones.

                            PREGUNTA DEL USUARIO: {request.UserMessage}";

                // 3. Enviamos el prompt al Repositorio que ya tienes configurado
                var response = await _aiRepo.GenerateTextAsync(prompt);

                return response;
            }
            catch (Exception ex)
            {
                // Loguear el error si es necesario
                return "Lo siento, hubo un problema al obtener los detalles del producto. ¿Puedes intentar de nuevo?";
            }
        }

    }
}