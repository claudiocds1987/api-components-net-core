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
            var intencionJson = await AnalyzeSearchAsync(preguntaUsuario);
            string terminoBusqueda = "";

            try
            {
                var jsonLimpio = intencionJson.Replace("```json", "").Replace("```", "").Trim();
                using var doc = JsonDocument.Parse(jsonLimpio);
                terminoBusqueda = doc.RootElement.GetProperty("busqueda").GetString() ?? "";
            }
            catch { terminoBusqueda = preguntaUsuario.Split(' ').Last().Replace("?", ""); }

            // 2. Buscamos en DummyJSON
            var dummyUrl = $"https://dummyjson.com/products/search?q={Uri.EscapeDataString(terminoBusqueda)}";
            var searchResult = await _httpClient.GetFromJsonAsync<DummyProductResponseDto>(dummyUrl);

            // 3. Formateamos el contexto con MÁS DETALLES (Stock, Descuento, etc.)
            string contextoCatalogo = "NO SE ENCONTRARON PRODUCTOS.";
            if (searchResult?.Products?.Any() == true)
            {
                // Tomamos los datos más relevantes para que la IA pueda responder dudas específicas
                contextoCatalogo = string.Join("\n", searchResult.Products.Take(2).Select(p =>
                    $"- Producto: {p.Title}\n" +
                    $"  Precio: {p.Price} USD (Descuento: {p.DiscountPercentage}%)\n" +
                    $"  Stock: {p.Stock} unidades disponibles\n" +
                    $"  Calificación: {p.Rating}/5\n" +
                    $"  Descripción: {p.Description}"));
            }

            // 4. PROMPT MAESTRO CON PROACTIVIDAD
            var promptMaestro = $@"
        Eres un asistente de ventas experto de 'IA-Store'.
        Tu fuente de verdad es este CATÁLOGO:
        {contextoCatalogo}

        REGLAS DE ORO:
        1. Si NO hay productos: Di que no lo encuentras y sugiere buscar algo similar.
        2. Si HAY productos: 
           - Da el precio y una característica llamativa.
           - Menciona brevemente que tienes stock disponible o el descuento si es alto.
        3. SIEMPRE termina tu respuesta con una pregunta proactiva relacionada con los datos del producto.
           Ejemplos: '¿Te gustaría saber si tiene algún descuento especial?', '¿Quieres que te confirme el stock disponible?', '¿Te interesa conocer la calificación de otros compradores?'.
        
        PREGUNTA DEL USUARIO: {preguntaUsuario}";

            return await _aiRepo.GenerateTextAsync(promptMaestro);
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