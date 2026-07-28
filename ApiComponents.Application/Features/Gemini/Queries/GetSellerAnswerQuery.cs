using ApiComponents.Application.Repositories;
using MediatR;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiComponents.Application.Features.Gemini.Queries;

/// <summary>
/// Genera una respuesta de vendedor experto para un producto específico.
/// 
/// ¿POR QUÉ SE NECESITA ESTA FUNCIÓN? (Motivación de Arquitectura):
/// Cuando el usuario ya está viendo un producto puntual en el frontend y quiere hacer una pregunta específica 
/// sobre ese producto, este handler busca los datos completos del producto en la base de datos,
/// los serializa como contexto para la IA, y solicita una respuesta breve y profesional de vendedor experto.
/// 
/// PARÁMETROS:
/// - ProductId: El ID del producto que el usuario seleccionó desde el componente de Angular (selectProduct()).
/// - UserMessage: El texto que el usuario escribe en el input al seleccionar el producto.
/// 
/// QUÉ HACE:
/// 1. Busca el producto por ID en el repositorio de productos.
/// 2. Si el producto no existe, devuelve un mensaje amigable indicándolo.
/// 3. Serializa el modelo completo del producto para que la IA lo lea como contexto.
/// 4. Construye un prompt con reglas de vendedor experto (brevedad, idioma del usuario, datos proporcionados).
/// 5. Delega la generación de texto al repositorio de IA (Gemini).
/// 
/// DEVUELVE (return):
/// Un string con la respuesta generada por la IA actuando como vendedor experto,
/// o un mensaje indicando que el producto no fue encontrado.
/// </summary>
public record GetSellerAnswerQuery(int ProductId, string UserMessage) : IRequest<string>;

public class GetSellerAnswerQueryHandler(
    IGeminiRepository aiRepo,
    IProductRepository productRepo) : IRequestHandler<GetSellerAnswerQuery, string>
{
    public async Task<string> Handle(GetSellerAnswerQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepo.GetProduct(request.ProductId, cancellationToken);

        if (product == null)
            return "Lo siento, no pude encontrar los detalles de ese producto en nuestro catálogo actual.";

        // Serializamos tu modelo real para que la IA lo lea
        var productData = JsonSerializer.Serialize(product, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });

        var prompt = $@"Eres un experto vendedor. Responde dudas sobre este producto: {productData}.
                                Reglas: 
                                - Responde en el idioma del usuario.
                                - Sé breve (máximo 3 frases).
                                - Usa solo los datos proporcionados.
                                Pregunta: {request.UserMessage}";

        return await aiRepo.GenerateTextAsync(prompt, cancellationToken);
    }
}
