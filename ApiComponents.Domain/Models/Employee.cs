using System.ComponentModel.DataAnnotations;

namespace ApiComponents.Domain.Models
{
    public class Employee
    {
        public int id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
        public string surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "El país es obligatorio.")]
        public int countryId { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        // Opcional: Podrías agregar un rango para evitar fechas imposibles
        public DateTime birthDate { get; set; }

        [Required(ErrorMessage = "El puesto/posición es obligatorio.")]
        public int positionId { get; set; }

        [Required]
        public bool active { get; set; } = true; // Por defecto activo

        [Url(ErrorMessage = "La URL de la imagen no es válida.")]
        public string imgUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "El género es obligatorio.")]
        public int genderId { get; set; }
    }
}