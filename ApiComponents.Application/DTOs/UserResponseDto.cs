namespace ApiComponents.Application.DTOs
{
    public class UserResponseDto
    {
        public int id { get; set; }
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty; // 'admin' o 'customer'
        public string token { get; set; } = string.Empty; // Aca viaja el JWT
    }
}
