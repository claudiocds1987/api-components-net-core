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
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public double DiscountPercentage { get; set; } // Agregado
        public int Stock { get; set; } // Agregado
        public double Rating { get; set; } // Agregado
        public string Brand { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Agregado para búsquedas por categoría
    }
}