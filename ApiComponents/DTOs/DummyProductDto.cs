namespace ApiComponents.DTOs
{
    // Este es el objeto principal que recibe la lista
    public class DummyProductResponseDto
    {
        public List<DummyProductDto> Products { get; set; } = new();
    }

    // Este es el detalle de cada producto que viene de DummyJSON
    public class DummyProductDto
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