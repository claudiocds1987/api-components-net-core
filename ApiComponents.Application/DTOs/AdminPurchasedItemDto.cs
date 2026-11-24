namespace ApiComponents.Application.DTOs;

/// <summary>
/// Representa un ítem de compra para el panel de Administrador ("Compras").
/// </summary>
public class AdminPurchasedItemDto
{
    public int orderId { get; set; }
    public int productId { get; set; }
    public string productTitle { get; set; } = string.Empty;
    public string productImage { get; set; } = string.Empty;
    public DateTime createdAt { get; set; }
    public decimal price { get; set; }
    public int quantity { get; set; }
    public string status { get; set; } = string.Empty;
    public int? userId { get; set; }
    public string customerName { get; set; } = string.Empty;
    public string customerEmail { get; set; } = string.Empty;
}
