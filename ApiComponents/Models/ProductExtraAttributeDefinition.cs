namespace ApiComponents.Models
{
    // Por ejemplo: "Memoria RAM", "Material", "Modelo" "Tipo de Micrófono".
    public class ProductExtraAttributeDefinition
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public int categoryId { get; set; }
        public string dataType { get; set; } = string.Empty; // 'text', 'number', 'boolean'


        public string? validationsJson { get; set; } // JSON: {"required": true, "maxLength": 200}

        // Relación inversa para EF
        public virtual ICollection<ProductExtraAttributeValue> attributeValues { get; set; } = new List<ProductExtraAttributeValue>();
    }
}
