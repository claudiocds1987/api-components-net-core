using System.ComponentModel.DataAnnotations;

namespace ApiComponents.Models
{
    public class ProductTag
    {
        public int id { get; set; }

        [Required(ErrorMessage = "El nombre del tag es obligatorio.")]
        [StringLength(200)]
        public string tagName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID del producto para crear el tag obligatorio.")]
        public int productId { get; set; }
    }
}
