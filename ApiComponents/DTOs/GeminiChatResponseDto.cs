using System.Collections.Generic;

namespace ApiComponents.DTOs
{
    public class GeminiChatResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public List<ProductDto> Products { get; set; } = new();
    }
}
