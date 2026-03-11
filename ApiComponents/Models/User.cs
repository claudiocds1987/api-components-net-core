namespace ApiComponents.Models
{
    public class User
    {
        public int id { get; set; }
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string passwordHash { get; set; } = string.Empty; // Nunca guardar texto plano
        public string role { get; set; } = "customer"; //'admin' | 'customer'
    }
}
