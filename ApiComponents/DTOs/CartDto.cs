using System.Collections.Generic;

namespace ApiComponents.DTOs
{
    // Este es el objeto principal que recibirá el Controller
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }

    // Esta clase representa a cada producto individual dentro del carrito
    public class CartItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}