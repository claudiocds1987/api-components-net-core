using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiComponents.Models
{
    public class Product
    {
        // id null, undefined o que el campo no venga (para el create product el id es generado por la DB, para el update product el id es obligatorio)
        public int? id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200)]
        public string title { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal price { get; set; }

        public decimal discountPercentage { get; set; }

        public decimal rating { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int stock { get; set; }

        [Required(ErrorMessage = "El SKU es obligatorio.")]
        public string sku { get; set; } = string.Empty;

        public decimal weight { get; set; }
        public decimal width { get; set; }
        public decimal height { get; set; }
        public decimal depth { get; set; }

        public string warrantyInformation { get; set; } = string.Empty;
        public string shippingInformation { get; set; } = string.Empty;
        public string availabilityStatus { get; set; } = "In Stock";
        public string returnPolicy { get; set; } = string.Empty;

        public int minimumOrderQuantity { get; set; } = 1;

        [Required(ErrorMessage = "La imagen principal es obligatoria.")]
        public string thumbnail { get; set; } = string.Empty;

        // IDs numéricos (Requeridos para el Frontend y la DB)
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public int categoryId { get; set; }

        [JsonIgnore]
        public ProductCategory? category { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria.")]
        public int brandId { get; set; }

        [JsonIgnore]
        public ProductBrand? brand { get; set; }

        // Tablas hijas inicializadas para evitar nulls
        public List<ProductImage> images { get; set; } = [];
        public List<ProductTag> tags { get; set; } = [];
        public List<ProductReview> reviews { get; set; } = [];

        /// <summary>
        /// Relación con los atributos dinámicos del producto (EAV)
        /// </summary>
        public ICollection<ProductExtraAttributeValue> extraAttributeValues { get; set; } = new List<ProductExtraAttributeValue>();

        public bool isActive { get; set; } = true;

        // --- Reglas de negocio (DDD) ---
        // Validaciones encapsuladas que pertenecen al agregado Product
        public void ValidateForCreate()
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new InvalidOperationException("El título es obligatorio.");

            if (price <= 0)
                throw new InvalidOperationException("El precio debe ser mayor a 0.");

            if (stock < 0)
                throw new InvalidOperationException("El stock no puede ser negativo.");

            if (minimumOrderQuantity < 1)
                minimumOrderQuantity = 1;
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0)
                throw new InvalidOperationException("El precio debe ser mayor a 0.");

            price = newPrice;
        }

        public void ChangeActiveState(bool active) => isActive = active;

        public void AddImage(ProductImage image)
        {
            if (image == null) return;
            images ??= new List<ProductImage>();
            images.Add(image);
        }

        public void AddTag(ProductTag tag)
        {
            if (tag == null) return;
            tags ??= new List<ProductTag>();
            tags.Add(tag);
        }
    }
}