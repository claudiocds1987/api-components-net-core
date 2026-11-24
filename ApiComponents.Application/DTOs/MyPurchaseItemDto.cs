namespace ApiComponents.Application.DTOs;

/// <summary>
/// Representa un producto comprado en la vista "Mis Compras" del cliente autenticado.
/// </summary>
public class MyPurchaseItemDto
{
    public int orderId { get; set; }
    public int productId { get; set; }
    public string productTitle { get; set; } = string.Empty;
    public string productImage { get; set; } = string.Empty;
    public int? categoryId { get; set; }
    public string? categoryName { get; set; }
    public DateTime createdAt { get; set; }
    public decimal price { get; set; }
    public int quantity { get; set; }
}
