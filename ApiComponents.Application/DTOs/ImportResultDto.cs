namespace ApiComponents.Application.DTOs
{
    public class ImportResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = [];
        public int Count { get; set; }
    }
}