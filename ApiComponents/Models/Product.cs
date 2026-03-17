using System.Text.Json.Serialization; // Necesario para JsonIgnore

namespace ApiComponents.Models
{
    public class Product
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public decimal discountPercentage { get; set; }
        public decimal rating { get; set; }
        public int stock { get; set; }
        public string sku { get; set; }
        public decimal weight { get; set; }
        public decimal width { get; set; }
        public decimal height { get; set; }
        public decimal depth { get; set; }
        public string warrantyInformation { get; set; }
        public string shippingInformation { get; set; }
        public string availabilityStatus { get; set; }
        public string returnPolicy { get; set; }
        public int minimumOrderQuantity { get; set; }
        public string thumbnail { get; set; }

        // IDs numéricos (Lo que viene en el Excel)
        public int categoryId { get; set; }

        [JsonIgnore] // Evita que aparezca 'category: null' y solo devuelva categoryId en el JSON
        public ProductCategory category { get; set; }

        public int brandId { get; set; }

        [JsonIgnore] // Evita que aparezca 'brand: null' y solo devuelva brandId en el JSON
        public ProductBrand brand { get; set; }

        // Tablas hijas (1:N)
        public List<ProductImage> images { get; set; } = new();
        public List<ProductTag> tags { get; set; } = new();

        public List<ProductReview> reviews { get; set; } = new();
    }
}