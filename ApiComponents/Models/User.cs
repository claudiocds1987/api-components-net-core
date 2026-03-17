using System.ComponentModel.DataAnnotations;

namespace ApiComponents.Models
{
    public class User
    {
        public int id { get; set; }

        [Required(ErrorMessage = "El username del usuario es obligatorio.")]
        [StringLength(200)]
        public string username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email del usuario es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(200)]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del usuario es obligatorio.")]
        [StringLength(200)]
        public string firstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido del usuario es obligatorio.")]
        [StringLength(200)]
        public string lastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El password del usuario es obligatorio.")]
        [StringLength(500)] // <--- Aumentado: los hashes (BCrypt/Identity) suelen ser largos
        public string passwordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol de usuario es obligatorio.")]
        [StringLength(50)]
        public string role { get; set; } = "customer";
    }
}