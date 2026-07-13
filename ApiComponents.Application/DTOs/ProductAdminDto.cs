namespace ApiComponents.Application.DTOs;

public class ProductAdminDto
{
    public int id { get; set; }
    public string title { get; set; } = string.Empty;
    public string sku { get; set; } = string.Empty;
    public decimal price { get; set; }

    public decimal discountPercentage { get; set; }
    public int stock { get; set; }
    public int categoryId { get; set; }
    public int brandId { get; set; }
    public bool isActive { get; set; }
    public string imageUrl { get; set; } = string.Empty;
}
