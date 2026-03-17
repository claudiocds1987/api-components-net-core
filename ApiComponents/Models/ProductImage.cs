using System.ComponentModel.DataAnnotations;

namespace ApiComponents.Models
{
    public class ProductImage
    {
        public int id { get; set; }

        [Required(ErrorMessage = "La URL de la imagen es obligatoria.")]
        // No le ponemos [Url] para que te deje usar rutas locales o nombres de archivos
        public string imageUrl { get; set; } = string.Empty;


        [Required(ErrorMessage = "El id del producto al que pertenece la imagen es obligatorio.")]
        public int productId { get; set; }
    }
}