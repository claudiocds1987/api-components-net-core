namespace ApiComponents.Application.DTOs;

public class ProductRequestDTo
{
    public int? id { get; set; }
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal price { get; set; }
    public decimal discountPercentage { get; set; }
    public decimal rating { get; set; }
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
    public bool isActive { get; set; } = true;

    // Simplified children DTOs
    public List<object>? images { get; set; }
    public List<object>? tags { get; set; }
    public List<object>? extraAttributes { get; set; }
}
