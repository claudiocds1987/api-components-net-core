using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace ApiComponents.Domain.Models
{
    public class ProductReview
    {
        [JsonIgnore] // <--- Para que Swagger no pida el id en el POST (no hace falta se genera automaticamente)
        public int id { get; set; }
        [Required(ErrorMessage = "La puntuación es obligatoria.")]
        [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5.")]
        public int rating { get; set; }

        [Required(ErrorMessage = "El comentario no puede estar vacío.")]
        [StringLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres.")]
        public string comment { get; set; } = string.Empty;

        [JsonIgnore]
        public DateTime date { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string userName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        public string userEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID del producto es obligatorio.")]
        public int productId { get; set; }
    }
}
