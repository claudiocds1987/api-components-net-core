namespace ApiComponents.Domain
{
    public class Order
    {
        public int id { get; set; }
        public string? preferenceId { get; set; } // Puede ser null al instanciar, se llena al conectar con MP
        public decimal totalAmount { get; set; }
        public string status { get; set; } = "Pending"; // Estado de la compra: Pending, Approved, Rejected, etc.
        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        // Nuevos campos para soportar invitados y envíos
        public int? userId { get; set; }
        public string customerEmail { get; set; } = null!;
        public string customerName { get; set; } = null!;
        public string? customerPhone { get; set; }
        public string shippingAddress { get; set; } = null!;
        public string shippingCity { get; set; } = null!;
        public string shippingZipCode { get; set; } = null!;

        // Relación Uno a Muchos con los detalles
        public ICollection<OrderDetail> orderDetails { get; set; } = new List<OrderDetail>();
    }
}