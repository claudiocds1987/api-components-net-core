namespace ApiComponents.Application.DTOs;

public class TotalProductsByBrandDto
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
}


