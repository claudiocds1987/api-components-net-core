namespace ApiComponents.Models
{
    // Por ejemplo: "Memoria RAM", "Material", "Modelo" "Tipo de Micrófono".
    public class ProductExtraAttributeDefinition
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;

        // Para saber a qué categoría pertenece esta definición
        public int categoryId { get; set; }

        // Para que Angular sepa qué input mostrar: "text", "number", "boolean"
        public string dataType { get; set; } = "text";

        public ICollection<ProductExtraAttributeValue> attributeValues { get; set; } = new List<ProductExtraAttributeValue>();
    }
}
