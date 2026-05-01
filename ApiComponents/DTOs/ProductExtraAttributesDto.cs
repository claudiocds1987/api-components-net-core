namespace ApiComponents.DTOs
{
    public class ProductExtraAttributesDto
    {
        public string name { get; set; } = string.Empty; // ID técnico (ej: "aroma")
        public string label { get; set; } = string.Empty; // Texto para el usuario (ej: "Aroma")
        public string dataType { get; set; } = string.Empty; // 'text', 'number', 'select', 'boolean'
        public bool required { get; set; } // Para validaciones dinámicas
        public List<string>? options { get; set; } // Opciones en caso de ser un 'select'
    }
}
