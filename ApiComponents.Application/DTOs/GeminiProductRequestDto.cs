namespace ApiComponents.Application.DTOs
{
    public class GeminiProductRequestDto
    {
        public int ProductId { get; set; } // El ID que mandamos desde el selectProduct() de Angular

        public string UserMessage { get; set; } = string.Empty; // El texto que el usuario escribe en el input al seleccionar el producto
    }
}