namespace ApiComponents.Application.DTOs;

/// <summary>
/// DTO público para exponer reviews a clientes.
/// No expone UserEmail por seguridad (OWASP: Sensitive Data Exposure).
/// </summary>
public class ProductReviewDto
{
    public int id { get; set; }
    public int rating { get; set; }
    public string comment { get; set; } = string.Empty;
    public string userName { get; set; } = string.Empty;
    public DateTime date { get; set; }
}
