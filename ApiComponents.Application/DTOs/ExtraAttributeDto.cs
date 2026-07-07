namespace ApiComponents.Application.DTOs
{
    public class ExtraAttributeDto
    {
        public string name { get; set; } = string.Empty; // Ej: "Color"
        public string value { get; set; } = string.Empty; // Ej: "Azul"

        public string label { get; set; } = string.Empty;
        public string dataType { get; set; } = string.Empty; // Ej: "text"
    }
}
