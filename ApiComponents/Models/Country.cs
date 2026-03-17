using System.ComponentModel.DataAnnotations;

namespace ApiComponents.Models
{
    public class Country
    {
        public int id { get; set; }

        [Required(ErrorMessage = "El nombre del país es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del país no puede superar los 100 caracteres.")]
        [MinLength(3, ErrorMessage = "El nombre del país debe tener al menos 3 caracteres.")]
        public string description { get; set; } = string.Empty;
    }
}