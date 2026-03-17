using System.ComponentModel.DataAnnotations;

namespace ApiComponents.Models
{
    public class ProductCategory
    {
        public int id { get; set; }
        [Required(ErrorMessage = "Elnombre de la categoría es obligatoria.")]
        [StringLength(200)]
        public string name { get; set; } = string.Empty;
    }
}
