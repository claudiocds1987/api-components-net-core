namespace ApiComponents.Domain.Models
{
    // Valor específico para un producto (Ej: "4K")
    public class ProductExtraAttributeValue
    {
        public int id { get; set; }
        public int productId { get; set; }
        public Product product { get; set; } = null!;

        public int attributeDefinitionId { get; set; }
        public ProductExtraAttributeDefinition attributeDefinition { get; set; } = null!;

        public string value { get; set; } = string.Empty;
    }
}
