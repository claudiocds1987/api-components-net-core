using System;

namespace ApiComponents.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string PreferenceId { get; set; } = string.Empty; // El ID de Mercado Pago
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // "Pending", "Success", "Failure"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
