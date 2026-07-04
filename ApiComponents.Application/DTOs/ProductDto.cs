namespace ApiComponents.Application.DTOs;

public class ProductDto
{
    public int id { get; set; }
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal price { get; set; }
    public decimal discountPercentage { get; set; }
    public decimal rating { get; set; }
    public int stock { get; set; }
    public string sku { get; set; } = string.Empty;
    public string thumbnail { get; set; } = string.Empty;
    public int categoryId { get; set; }
    public int brandId { get; set; }
}
