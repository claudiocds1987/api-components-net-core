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

        public async Task<GeminiChatResponseDto> ConsultarConCatalogoAsync(string preguntaUsuario)
        {
            // 1. Descargar todo el catálogo
            var allProductsUrl = "https://dummyjson.com/products?limit=100";
            var allProductsResult = await _httpClient.GetFromJsonAsync<DummyProductResponseDto>(allProductsUrl);
            var allProducts = allProductsResult?.Products ?? new List<DummyProductDto>();

            string lowerQuery = preguntaUsuario.ToLower();
            var sinonimosFragancias = new[] {
                "fragancia", "fragancias", "perfume", "perfumes", "aroma", "aromas",
                "oler bien", "buen aroma", "buen olor", "aromatizar", "fragante", "esencia"
            };
            var sinonimosMaquillaje = new[] { "maquillaje", "makeup" };
            var sinonimosSkincare = new[] { "skincare", "skin-care", "cuidado de la piel" };
            var sinonimosOfertas = new[] { "oferta", "ofertas", "descuento", "descuentos", "mejor precio" };

            // Lista de categorías reales del catálogo
            var categoriasCatalogo = new[]
            {
                "beauty", "fragrances", "furniture", "groceries", "home-decoration", "kitchen-accessories",
                "laptops", "mens-shirts", "mens-shoes", "mens-watches", "mobile-accessories", "motorcycle",
                "skin-care", "smartphones", "sports-accessories", "sunglasses", "tablets", "tops", "vehicle",
                "womens-bags", "womens-dresses", "womens-jewellery", "womens-shoes", "womens-watches"
            };

            List<DummyProductDto> productos = new();
            string categoriaDetectada = "";
            bool esOferta = sinonimosOfertas.Any(s => lowerQuery.Contains(s));

            // 1. Búsqueda por coincidencia exacta de título (normalizado)
            string Normalizar(string s) =>
                s.ToLower()
                 .Replace("-", "")
                 .Replace(" ", "")
                 .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                 .Replace("?", "").Replace(".", "").Replace(",", "");

            var promptNormalizado = Normalizar(preguntaUsuario);

            // Buscar productos cuyo título coincida exactamente (normalizado)
            var productosPorTitulo = allProducts
                .Where(p => Normalizar(p.Title) == promptNormalizado)
                .ToList();

            if (productosPorTitulo.Any())
            {
                return new GeminiChatResponseDto
                {
                    Response = "¡Sí, por supuesto! Aquí tienes el producto encontrado:",
                    Products = productosPorTitulo
                };
            }

            // 2. Detección automática de categoría por coincidencia robusta y normalizada en el prompt (prompt completo)
            string categoriaPrompt = categoriasCatalogo.FirstOrDefault(cat =>
                promptNormalizado.Contains(Normalizar(cat)) ||
                Normalizar(cat).Contains(promptNormalizado) ||
                promptNormalizado.Contains(Normalizar(cat).TrimEnd('s')) || // singular
                promptNormalizado.Contains(Normalizar(cat) + "s")          // plural
            );

            if (!string.IsNullOrEmpty(categoriaPrompt))
            {
                productos = allProducts.Where(p => Normalizar(p.Category) == Normalizar(categoriaPrompt)).ToList();
                categoriaDetectada = categoriaPrompt.Replace("-", " ");
            }
            else if (sinonimosFragancias.Any(s => lowerQuery.Contains(s)))
            {
                productos = allProducts.Where(p => p.Category.ToLower() == "fragrances").ToList();
                categoriaDetectada = "fragancias";
            }
            else if (sinonimosMaquillaje.Any(s => lowerQuery.Contains(s)))
            {
                productos = allProducts.Where(p => p.Category.ToLower() == "beauty").ToList();
                categoriaDetectada = "maquillaje";
            }
            else if (sinonimosSkincare.Any(s => lowerQuery.Contains(s)))
            {
                productos = allProducts.Where(p => p.Category.ToLower().Contains("skin")) .ToList();
                categoriaDetectada = "skincare";
            }
            else if (esOferta)
            {
                productos = allProducts.OrderByDescending(p => p.DiscountPercentage).Take(10).ToList();
                categoriaDetectada = "ofertas";
            }
            else
            {
                // Búsqueda general en title, description, category, brand
                productos = allProducts.Where(p =>
                    lowerQuery.Split(' ').Any(q =>
                        p.Title.ToLower().Contains(q) ||
                        p.Description.ToLower().Contains(q) ||
                        p.Category.ToLower().Contains(q) ||
                        p.Brand.ToLower().Contains(q)
                    )
                ).ToList();
                categoriaDetectada = productos.FirstOrDefault()?.Category ?? "producto";
            }

            // 3. Lógica de metadata dinámica
            foreach (var prod in productos)
            {
                var meta = "";
                if (prod.Stock < 5) meta += " ⚠️ ¡Últimas unidades!";
                if (prod.Rating > 4.5) meta += " ⭐ Favorito de los clientes";
                if (!string.IsNullOrWhiteSpace(meta))
                    prod.Description = prod.Description.Trim() + meta;
            }

            // 4. Lógica de respuesta
            if (productos.Any())
            {
                string responseMsg = esOferta
                    ? "¡Claro! Estos son nuestros productos con los mejores descuentos hoy:"
                    : $"¡Sí, por supuesto! Aquí tienes las opciones de {categoriaDetectada.ToLower()}:";
                // Ordenar por descuento si es oferta
                if (esOferta)
                    productos = productos.OrderByDescending(p => p.DiscountPercentage).ToList();
                return new GeminiChatResponseDto
                {
                    Response = responseMsg,
                    Products = productos
                };
            }
            else
            {
                // Sugerir categoría relacionada si no hay resultados
                string sugerir = "fragancias";
                if (sinonimosFragancias.Any(s => lowerQuery.Contains(s))) sugerir = "maquillaje";
                else if (sinonimosMaquillaje.Any(s => lowerQuery.Contains(s))) sugerir = "skincare";
                else if (sinonimosSkincare.Any(s => lowerQuery.Contains(s))) sugerir = "fragancias";
                else if (esOferta) sugerir = "fragancias";
                return new GeminiChatResponseDto
                {
                    Response = $"No encontré {preguntaUsuario} en el catálogo. ¿Te gustaría ver {sugerir}?",
                    Products = new List<DummyProductDto>()
                };
            }
        }

        public async Task<string> AnalyzeSearchAsync(string text)
        {
            var prompt = $@"Analiza la pregunta del usuario y extrae exclusivamente el NOMBRE del producto o marca que busca.\n                         Usuario: '{text}'\n                         Responde SOLO un JSON: {{""busqueda"": ""valor""}}\n                         Ejemplo: 'tienes algo de essence?' -> {{""busqueda"": ""essence""}}";

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