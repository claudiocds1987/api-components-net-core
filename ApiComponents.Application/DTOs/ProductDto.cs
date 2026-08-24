namespace ApiComponents.Application.DTOs;

public class ProductDto
{
    public int id { get; set; }
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal price { get; set; }
    public double discountPercentage { get; set; }
    public int stock { get; set; }
    public double rating { get; set; }
    public string brand { get; set; } = string.Empty;
    public string thumbnail { get; set; } = string.Empty;
    public string category { get; set; } = string.Empty;
    public List<ProductTagDto> tags { get; set; } = [];
    public bool isActive { get; set; }
}
