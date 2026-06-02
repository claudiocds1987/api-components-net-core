
namespace ApiComponents.DTOs
{
    public class CartDto
    {
        public int? userId { get; set; } // NULL si es usuario invitado
        public string customerEmail { get; set; } = null!;
        public string customerName { get; set; } = null!;
        public string? customerPhone { get; set; }
        public string shippingAddress { get; set; } = null!;
        public string shippingCity { get; set; } = null!;
        public string shippingZipCode { get; set; } = null!;
        public List<CartItemDto> items { get; set; } = new();
    }

    public class CartItemDto
    {
        public int productId { get; set; }
        public int quantity { get; set; }
    }
}