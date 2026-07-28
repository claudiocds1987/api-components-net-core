using ApiComponents.Application.DTOs;
using ApiComponents.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiComponents.Application.Features.Gemini.Queries;

/// <summary>
/// Orquesta el flujo principal de búsqueda inteligente en el catálogo, interactuando con la base de datos, 
/// el motor semántico de IA y formateando la respuesta final para el cliente.
/// 
/// ¿POR QUÉ SE NECESITA ESTA FUNCIÓN? (Motivación de Arquitectura):
/// Es el punto de entrada público (API Entrypoint) para el chat semántico de nuestro sistema. 
/// Se necesita para unificar en un solo flujo asincrónico la obtención masiva de productos indexados, 
/// la reducción drástica de datos para la optimización de tokens de IA (ahorro de costos y ancho de banda), 
/// la delegación de la búsqueda semántica y, finalmente, la reconstrucción ordenada de los DTOs que consumirá el frontend.
/// 
/// PARÁMETROS:
/// - UserQuestion: La consulta literal o pregunta en lenguaje natural enviada por el usuario desde la interfaz de chat.
/// 
/// QUÉ HACE:
/// 1. Consulta la base de datos trayendo los primeros 250 productos activos mapeados a un formato plano (ProductDto).
/// 2. Construye un sub-objeto anónimo ultra-liviano (removiendo campos pesados como imágenes o descripciones largas) 
///    para alimentar a la IA gastando el menor presupuesto de tokens posible.
/// 3. Solicita a los componentes internos el análisis inteligente de coincidencia (GetSemanticMatchesFromAI).
/// 4. Utiliza LINQ para cruzar y ordenar los productos reales de la base de datos basándose exclusivamente en 
///    los IDs y puntajes de relevancia (Score) elegidos por el motor de búsqueda.
/// 5. Evalúa el resultado final para construir una respuesta adaptativa amigable tanto para búsquedas exitosas como vacías.
/// 
/// DEVUELVE (return):
/// Un objeto de transferencia de datos <see cref="GeminiChatResponseDto"/> que encapsula un mensaje conversacional 
/// de respuesta (Response) y el listado de productos (<see cref="List{ProductDto}"/>) ordenados por relevancia listos para renderizar.
/// </summary>
public record QueryCatalogQuery(string UserQuestion) : IRequest<GeminiChatResponseDto>;

public class QueryCatalogQueryHandler(
    IGeminiRepository aiRepo,
    IProductRepository productRepo,
    ILogger<QueryCatalogQueryHandler> logger) : IRequestHandler<QueryCatalogQuery, GeminiChatResponseDto>
{
    public async Task<GeminiChatResponseDto> Handle(QueryCatalogQuery request, CancellationToken cancellationToken)
    {
        // 1. Traemos los productos (solo los campos que Gemini necesita leer)
        var dbResult = await productRepo.GetProductsAsync(page: 1, size: 250, isActive: true, cancellationToken: cancellationToken);

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
            tags = p.tags != null ? p.tags.Select(t => new ProductTagDto { id = t.id ?? 0, tagName = t.tagName }).ToList() : []
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
        var matchedIds = await GetSemanticMatchesFromAI(request.UserQuestion, catalogJson, allProducts, cancellationToken);

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
                Response = $"¡Claro! Encontré {filteredProducts.Count} opciones para \"{request.UserQuestion}\":",
                Products = filteredProducts!
            };
        }

        return new GeminiChatResponseDto
        {
            Response = $"No encontré productos para \"{request.UserQuestion}\". ¿Podés intentar con otros términos?",
            Products = new List<ProductDto>()
        };
    }

    /// <summary>
    /// Coordina de manera integral el flujo de búsqueda semántica consumiendo los servicios cognitivos de Gemini 
    /// y gestionando la recuperación local en caso de error.
    /// 
    /// ¿POR QUÉ SE NECESITA ESTA FUNCIÓN? (Motivación de Arquitectura):
    /// Es el punto de entrada principal y el "Orquestador" de la búsqueda inteligente en nuestro servicio. 
    /// Se necesita para centralizar la secuencia lógica del proceso: estructuración de la consulta, comunicación remota, 
    /// limpieza de datos y transformación de la respuesta. Además, actúa como el supervisor de resiliencia del sistema, 
    /// ya que implementa bloques try-catch anidados estratégicamente para atrapar cualquier error de red o de parseo, 
    /// garantizando que si algo falla, se active el motor de fallback local inmediatamente sin propagar errores al controlador.
    /// 
    /// PARÁMETROS:
    /// - userQuestion: La consulta en lenguaje natural realizada por el cliente (ej: "¿Tenés zapatillas running de mujer?").
    /// - catalogJson: El catálogo resumido y optimizado de productos en formato JSON que se inyectará en el prompt.
    /// - products: La lista de DTOs en memoria que se usará como insumo en caso de necesitar el motor local.
    /// - cancellationToken: Token para propagar notificaciones de que la operación debe cancelarse (ej: si el usuario cierra la pestaña).
    /// 
    /// QUÉ HACE:
    /// 1. Llama a la función generadora para construir el prompt con sus instrucciones comerciales.
    /// 2. Invoca asincrónicamente el repositorio de Inteligencia Artificial para obtener la predicción de texto.
    /// 3. Normaliza la respuesta descartando envolturas de Markdown (```json).
    /// 4. Delega el aislamiento del array y el parseo de objetos a las sub-funciones especializadas.
    /// 5. Valida si la IA devolvió resultados; si la lista está vacía, dispara defensivamente el fallback local.
    /// 
    /// DEVUELVE (return):
    /// Una colección (<see cref="List{ProductMatch}"/>) que contiene las coincidencias y puntajes asignados, 
    /// ya sea calculados de forma semántica por Gemini o resueltos de forma local por el algoritmo de errores.
    /// </summary>
    private async Task<List<ProductMatch>> GetSemanticMatchesFromAI(string userQuestion, string catalogJson, List<ProductDto> products, CancellationToken cancellationToken = default)
    {
        var prompt = BuildSemanticPrompt(userQuestion, catalogJson);

        try
        {
            var rawResponse = await aiRepo.GenerateTextAsync(prompt, cancellationToken);

            var cleanJson = (rawResponse ?? string.Empty)
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            try
            {
                var arrayJson = ExtractJsonArray(cleanJson);
                var matches = ParseMatchesFromJson(arrayJson);

                logger.LogDebug("Matches count: {Count}", matches.Count);

                if (!matches.Any())
                {
                    logger.LogDebug("AI returned no matches, running local fallback matching.");
                    return ExecuteLocalFallbackMatching(userQuestion, products);
                }

                return matches;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error parsing 'matches' from AI response. Forcing local fallback.");
                return ExecuteLocalFallbackMatching(userQuestion, products);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error comunicándose con el repositorio de Gemini. Forcing local fallback.");
            return ExecuteLocalFallbackMatching(userQuestion, products);
        }
    }

    /// <summary>
    /// Construye el prompt con las instrucciones de búsqueda semántica y las reglas de formateo para Gemini.
    /// </summary>
    private static string BuildSemanticPrompt(string userQuestion, string catalogJson)
    {
        return $@"Sos un motor de búsqueda de productos. Tu tarea es encontrar los productos más relevantes para la consulta del usuario.

            CATÁLOGO (JSON):
            {catalogJson}

            CONSULTA DEL USUARIO: ""{userQuestion}""

            INSTRUCCIONES:
            - Analizá semánticamente la consulta: detectá categoría, género, color, material, estilo, uso, marca, precio aproximado, etc.
            - Traducí mentalmente los términos (""plateado"" = silver, ""reloj"" = watch, ""mujer"" = womens, ""perfume"" = fragrance, etc.)
            - IMPORTANTE SOBRE CATEGORÍAS: En nuestro catálogo, las categorías compuestas usan GUIONES MEDIOS en lugar de espacios. Por ejemplo: ""smart tv"" es ""smart-tv"", ""relojes de hombre"" o ""mens watches"" es ""mens-watches"", ""accesorios de cocina"" es ""kitchen-accessories"". Tené esto muy en cuenta al evaluar coincidencias en el campo 'category'.
            - Buscá coincidencias en title, brand, category y tags de cada producto.
            - Excluí productos del género opuesto si la consulta especifica género.
            - Asigná un score de 0 a 100 según relevancia.
            - Devolvé ÚNICAMENTE un JSON válido con este formato exacto, sin texto adicional:
            {{""matches"": [{{""id"": 1, ""score"": 95}}, {{""id"": 2, ""score"": 80}}]}}
            - Si no hay resultados relevantes devolvé: {{""matches"": []}}
            - Máximo 20 resultados, solo los de score >= 40.";
    }

    /// <summary>
    ///  Aísla y extrae la sección de texto correspondiente al array JSON de coincidencias enviado por la IA.
    /// ¿POR QUÉ SE NECESITA ESTA FUNCIÓN? (Motivación de Arquitectura):
    /// Aunque se le pide a Gemini que devuelva únicamente un JSON limpio, los modelos de lenguaje a veces incluyen 
    /// texto conversacional no deseado antes o después del bloque JSON (ej: "¡Por supuesto! Aquí tienes el JSON: ...").
    /// Si intentamos parsear ese texto directamente, el sistema fallaría con un error crítico de sintaxis. 
    /// Esta función se necesita como un escudo de tolerancia a fallos para "recortar" con precisión quirúrgica 
    /// el fragmento de texto exacto del array (delimitado por [ y ]) y descartar cualquier basura textual externa.
    /// 
    /// PARÁMETRO (cleanJson): 
    /// Recibe una cadena de texto (string) que representa la respuesta cruda de la IA (ya libre de bloques de código markdown ```json).
    /// Puede contener texto plano mezclado con la estructura del JSON, por ejemplo:
    /// "Aquí está el resultado: { "matches": [ { "id": 1, "score": 90 } ] } - Espero te sirva."
    /// 
    /// QUÉ HACE: 
    /// 1. Busca de manera insensible a mayúsculas la clave string "\"matches\"" para ubicar dónde empiezan los datos relevantes.
    /// 2. A partir de esa posición, localiza el índice del primer corchete de apertura '['.
    /// 3. Luego busca el corchete de cierre correspondiente ']'.
    /// 4. Si encuentra ambos, recorta la cadena (Substring) extrayendo únicamente lo que hay entre esos corchetes (incluyéndolos).
    /// 
    /// DEVUELVE (return): 
    /// Un sub-string limpio que contiene únicamente el array de elementos (ej: "[ { "id": 1, "score": 90 } ]").
    /// Si el texto no incluye la palabra clave o los corchetes esperados, la función devuelve el string 'cleanJson' original 
    /// intacto de forma defensiva, permitiendo que el parser intente procesarlo de todas formas.
    /// </summary>
    private static string ExtractJsonArray(string cleanJson)
    {
        var lower = cleanJson.ToLowerInvariant();
        var matchesKey = "\"matches\"";
        var idx = lower.IndexOf(matchesKey, StringComparison.Ordinal);

        if (idx >= 0)
        {
            var arrayStart = cleanJson.IndexOf('[', idx);
            if (arrayStart >= 0)
            {
                var arrayEnd = cleanJson.IndexOf(']', arrayStart);
                if (arrayEnd > arrayStart)
                {
                    return cleanJson.Substring(arrayStart, arrayEnd - arrayStart + 1);
                }
            }
        }

        return cleanJson;
    }

    /// <summary>
    /// Parsea y convierte el texto JSON dinámico enviado por la IA en objetos nativos de C#.
    /// ¿POR QUÉ SE NECESITA ESTA FUNCIÓN? (Motivación de Arquitectura):
    /// Los modelos de lenguaje (LLMs) como Gemini devuelven respuestas puramente en texto plano (strings). 
    /// Para que nuestro backend en C# pueda operar, ordenar, paginar o filtrar los productos utilizando LINQ, 
    /// es obligatorio transformar ese texto plano en objetos fuertemente tipados en memoria. 
    /// Además, se necesita encapsular esta lógica aquí para aislar al resto del sistema de las variaciones o 
    /// inconsistencias menores que la IA pueda tener en el formateo de sus respuestas JSON de una petición a otra.
    /// 
    /// PARÁMETRO (arrayJson): 
    /// Recibe una cadena de texto (string) que contiene los datos serializados de la búsqueda. 
    /// Puede venir estructurado como un objeto raíz con la propiedad "matches" (ej: { "matches": [{ "id": 1, "score": 90 }] }) 
    /// o directamente como un array plano de elementos (ej: [{ "id": 1, "score": 90 }]).
    /// 
    /// QUÉ HACE: 
    /// Analiza el texto mediante JsonDocument. Identifica qué tipo de estructura JSON ingresó (Objeto o Array) 
    /// para recorrer los nodos internos de forma segura, delegando la extracción de los campos "id" y "score" al mapeador tipado.
    /// 
    /// DEVUELVE (return matches): 
    /// Una lista en memoria (<see cref="List{ProductMatch}"/>) fuertemente tipada. Cada objeto de la lista 
    /// contiene las propiedades numéricas 'Id' y 'Score' listas para ser utilizadas en filtros de C#. 
    /// Si el JSON de entrada no contiene coincidencias o es inválido, devuelve una lista limpia con cero elementos (vacía).
    /// </summary>
    private static List<ProductMatch> ParseMatchesFromJson(string arrayJson)
    {
        var matches = new List<ProductMatch>();
        using var doc = JsonDocument.Parse(arrayJson);

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var match = MapElementToProductMatch(el);
                if (match != null) matches.Add(match);
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            if (doc.RootElement.TryGetProperty("matches", out var matchesEl) && matchesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in matchesEl.EnumerateArray())
                {
                    var match = MapElementToProductMatch(el);
                    if (match != null) matches.Add(match);
                }
            }
        }

        return matches;
    }

    /// <summary>
    /// Mapea y convierte un elemento JSON dinámico en un objeto fuertemente tipado de tipo ProductMatch.
    /// 
    /// ¿POR QUÉ SE NECESITA ESTA FUNCIÓN? (Motivación de Arquitectura):
    /// Los modelos de IA no siempre son consistentes con los tipos de datos primitivos en sus respuestas JSON. 
    /// Por ejemplo, en una petición pueden devolver el ID como un número (id: 12) y en la siguiente como un texto (id: "12"). 
    /// Si intentamos mapear el JSON de forma rígida y directa, el backend arrojaría excepciones de casteo y se caería. 
    /// Esta función se necesita como un normalizador tolerante a tipos dinámicos, asegurando que sin importar 
    /// si el dato viene como Number o como String, se procese correctamente sin romper la estabilidad del sistema.
    /// 
    /// PARÁMETRO (el): 
    /// Recibe un objeto <see cref="JsonElement"/> que representa un nodo individual e indeterminado dentro del JSON.
    /// Conceptualmente representa una estructura similar a: { "id": 14, "score": 95 } o { "id": "14", "score": "95" }.
    /// 
    /// QUÉ HACE: 
    /// 1. Intenta leer de forma segura la propiedad "id". Si existe, verifica si es de tipo numérico o texto, 
    ///    extrayendo el valor entero correspondiente (int).
    /// 2. Intenta leer de forma segura la propiedad "score". Realiza la misma validación de tipo numérico o texto, 
    ///    extrayendo el valor con decimales (double).
    /// 3. Valida que el ID extraído sea válido (distinto de cero).
    /// 
    /// DEVUELVE (return): 
    /// Un objeto <see cref="ProductMatch"/> en memoria con sus propiedades 'Id' y 'Score' correctamente asignadas.
    /// Si la propiedad "id" no existía o no pudo ser parseada a un entero válido, devuelve 'null', indicando 
    /// al llamador que ese elemento en particular debe descartarse por no ser íntegro.
    /// </summary>
    private static ProductMatch? MapElementToProductMatch(JsonElement el)
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

        return id != 0 ? new ProductMatch { Id = id, Score = score } : null;
    }

    /// <summary>
    /// Ejecuta un algoritmo de búsqueda secundario basado en tokens, sinónimos y pesos de coincidencia textual.
    /// 
    /// ¿POR QUÉ SE NECESITA ESTA FUNCIÓN? (Motivación de Arquitectura):
    /// El sistema de búsqueda principal depende de una API de Inteligencia Artificial externa (Gemini). 
    /// En escenarios reales de producción, esa API puede fallar (por caídas del servicio, problemas de conectividad, 
    /// agotamiento de cuotas por exceso de peticiones o respuestas mal formateadas). 
    /// Esta función se necesita como una red de seguridad local ("Circuit Breaker" o Plan B) para garantizar la alta 
    /// disponibilidad del sistema, permitiendo que el catálogo web siga devolviendo resultados sumamente precisos 
    /// y relevantes sin que el usuario final perciba que la inteligencia artificial sufrió un contratiempo.
    /// 
    /// PARÁMETROS:
    /// - userQuestion: La consulta original escrita por el usuario en texto plano (ej: "Smart-TV de oferta").
    /// - products: La lista completa de productos activos cargados en la memoria desde el repositorio de la base de datos.
    /// 
    /// QUÉ HACE:
    /// 1. Normaliza la pregunta eliminando guiones y convirtiéndola a minúsculas para evitar problemas de concordancia literal.
    /// 2. Divide la consulta en palabras individuales (tokens), descartando conectores irrelevantes (stopwords como "de", "la").
    /// 3. Expande los criterios de búsqueda inyectando sinónimos específicos e inglés/español para mapear variaciones comunes (ej: "reloj" -> "watch").
    /// 4. Protege palabras técnicas cruciales de dos letras (como "tv" o "pc") para que no sean erróneamente ignoradas.
    /// 5. Recorre cada producto calculando un puntaje (score) acumulativo si se encuentran coincidencias en campos clave (title, brand, category, tags).
    /// 
    /// DEVUELVE (return):
    /// Una lista ordenada en forma descendente por relevancia (<see cref="List{ProductMatch}"/>) que contiene un máximo 
    /// de 20 productos cuyo score acumulado sea igual o superior al umbral mínimo de aceptación (40 puntos).
    /// </summary>
    private static List<ProductMatch> ExecuteLocalFallbackMatching(string userQuestion, List<ProductDto> products)
    {
        var fallback = new List<ProductMatch>();
        var cleanQuestion = userQuestion.Replace("-", " ").ToLowerInvariant();
        var rawTokens = cleanQuestion.Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

        var stopwords = new HashSet<string> { "de", "del", "la", "el", "los", "las", "para", "con", "y", "a" };
        var synonyms = new Dictionary<string, string[]>
        {
            { "mujer", new[] { "woman", "women", "womens", "female", "femenino" } },
            { "hombre", new[] { "man", "men", "mens", "male", "masculino" } },
            { "reloj", new[] { "watch", "timepiece" } },
            { "relojes", new[] { "watch", "watches" } },
            { "plateado", new[] { "silver" } },
            { "dorado", new[] { "gold", "golden" } },
            { "negro", new[] { "black" } },
            { "blanco", new[] { "white" } },
            { "perfume", new[] { "fragrance", "perfume" } }
        };

        var validShortTokens = new HashSet<string> { "tv", "pc", "3d", "4k", "hd", "it", "ai" };

        var tokens = new List<string>();
        foreach (var t in rawTokens)
        {
            if (t.Length <= 2 && !validShortTokens.Contains(t)) continue;
            if (stopwords.Contains(t)) continue;
            tokens.Add(t);
            if (synonyms.TryGetValue(t, out var syns)) tokens.AddRange(syns);
        }

        foreach (var p in products)
        {
            var originalHay = ((p.title ?? string.Empty) + " " +
                               (p.brand ?? string.Empty) + " " +
                               (p.category ?? string.Empty) + " " +
                               string.Join(" ", p.tags ?? new List<ProductTagDto>())).ToLowerInvariant();

            var normalizedHay = originalHay.Replace("-", " ");
            var completeHay = originalHay + " " + normalizedHay;

            var score = 0.0;
            foreach (var t in tokens.Distinct())
            {
                if (completeHay.Contains(t)) score += 25;
            }

            if (score >= 40)
            {
                fallback.Add(new ProductMatch { Id = p.id, Score = score });
            }
        }

        return fallback.OrderByDescending(f => f.Score).Take(20).ToList();
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
