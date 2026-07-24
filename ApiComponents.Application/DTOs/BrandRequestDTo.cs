using System.ComponentModel.DataAnnotations;

namespace ApiComponents.Application.DTOs;

public class BrandRequestDTo
{
    public int id { get; set; }

    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(200)]
    public string name { get; set; } = string.Empty;
    public bool isActive { get; set; } = true;
}
