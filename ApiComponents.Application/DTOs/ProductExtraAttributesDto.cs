namespace ApiComponents.Application.DTOs
{

    public class ProductExtraAttributesDto
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;  // ej: "aroma"
        public string label { get; set; } = string.Empty;  // Texto para el usuario (ej: "Aroma")
        public string dataType { get; set; } = string.Empty; // 'text', 'number', 'select', 'boolean'
        public int categoryId { get; set; }

        // Este objeto se mapeará desde/hacia el validationsJson del modelo
        public ExtraAttributeValidationsDto validations { get; set; } = new(); // validations.required, validations.maxLength, etc. para Angular
    }
}
