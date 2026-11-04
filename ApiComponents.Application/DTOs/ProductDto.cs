namespace ApiComponents.Application.DTOs;

public class ProductDto
{
    public int id { get; set; }
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal price { get; set; }
    public decimal discountPercentage { get; set; }
    public int stock { get; set; }

    // Estadísticas precalculadas — se leen directamente desde la tabla Products.
    // Usar decimal para soportar valores como 4.25, 4.75, etc.
    public decimal rating { get; set; }
    public int reviewCount { get; set; }

    public int brandId { get; set; }
    public string thumbnail { get; set; } = string.Empty;
    public int categoryId { get; set; }
    public List<ProductTagDto> tags { get; set; } = [];
    public bool isActive { get; set; }
}
