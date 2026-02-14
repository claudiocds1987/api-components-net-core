namespace ApiComponents.DTOs
{
    public class GeminiProductRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public object? Context { get; set; } // Aquí llega el JSON de dummys.json
    }
}