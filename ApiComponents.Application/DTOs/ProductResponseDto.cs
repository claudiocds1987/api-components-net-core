using ApiComponents.Domain.Models;

namespace ApiComponents.Application.DTOs;

public class ProductResponseDto
{
    public int id { get; set; }
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal price { get; set; }
    public decimal discountPercentage { get; set; }

    // Estadísticas precalculadas — se leen directamente desde la tabla Products.
    public decimal rating { get; set; }
    public int reviewCount { get; set; }

    public int stock { get; set; }
    public string sku { get; set; } = string.Empty;
    public decimal weight { get; set; }
    public decimal width { get; set; }
    public decimal height { get; set; }
    public decimal depth { get; set; }
    public string warrantyInformation { get; set; } = string.Empty;
    public string shippingInformation { get; set; } = string.Empty;
    public string availabilityStatus { get; set; } = "In Stock";
    public string returnPolicy { get; set; } = string.Empty;

    public int minimumOrderQuantity { get; set; } = 1;
    public string thumbnail { get; set; } = string.Empty;
    public int categoryId { get; set; }
    public int brandId { get; set; }
    public bool isActive { get; set; }

    // Listas de relaciones
    public List<ProductImage> images { get; set; } = [];
    public List<ProductTag> tags { get; set; } = [];

    // Seguridad: se usa ProductReviewDto para no exponer UserEmail en endpoints públicos.
    public List<ProductReviewDto> reviews { get; set; } = [];

    public List<ExtraAttributeDto> extraAttributes { get; set; } = [];
}
