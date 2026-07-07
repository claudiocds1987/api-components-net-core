namespace ApiComponents.Domain.Models
{
    public class OrderDetail
    {
        public int id { get; set; }
        public int orderId { get; set; }
        public int productId { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; } // Precio histórico congelado

        // Propiedades de navegación de EF Core
        public Order order { get; set; } = null!;
        // Si tenés la entidad Product mapeada:
        // public Product Product { get; set; } = null!;
    }
}