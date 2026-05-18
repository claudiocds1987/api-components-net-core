namespace ApiComponents.DTOs
{
    public class ProductDto
    {
        public int id { get; set; }
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public decimal price { get; set; }
        public double discountPercentage { get; set; } // Agregado
        public int stock { get; set; } // Agregado
        public double rating { get; set; } // Agregado
        public string brand { get; set; } = string.Empty;
        public string thumbnail { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty; // Agregado para búsquedas por categoría
        public List<string> tags { get; set; } = new(); // Agregado para búsqueda por tags
    }
}