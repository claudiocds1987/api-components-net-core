namespace ApiComponents.DTOs
{
    public class ExtraAttributeValidationsDto
    {
        // --- Común para todos los tipos ---
        public bool required { get; set; } = false;

        // --- Específicos para 'text' ---
        public int? minLength { get; set; }
        public int? maxLength { get; set; }

        // Para validar formatos específicos (ej: ^[a-zA-Z]+$)
        public string? pattern { get; set; }

        // --- Específicos para 'number' ---
        // Usamos double? para que acepte tanto enteros como decimales
        public double? min { get; set; }
        public double? max { get; set; }
    }
}
