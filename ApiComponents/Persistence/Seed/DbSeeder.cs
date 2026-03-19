using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Persistence.Seed
{
    public static class DbSeeder
    {
        // Método principal que orquestará todos los seeds
        public static async Task SeedAll(AppDbContext context)
        {
            await SeedCategories(context);
            await SeedBrands(context);
        }

        private static async Task SeedCategories(AppDbContext context)
        {
            if (!context.ProductCategories.Any())
            {
                var categories = new List<ProductCategory>
                {
                    new() { name = "beauty" },
                    new() { name = "fragrances" },
                    new() { name = "furniture" },
                    new() { name = "groceries" },
                    new() { name = "home-decoration" },
                    new() { name = "kitchen-accessories" },
                    new() { name = "laptops" },
                    new() { name = "mens-shirts" },
                    new() { name = "mens-shoes" },
                    new() { name = "mens-watches" },
                    new() { name = "mobile-accessories" },
                    new() { name = "motorcycle" },
                    new() { name = "skin-care" },
                    new() { name = "smartphones" },
                    new() { name = "sports-accessories" },
                    new() { name = "sunglasses" },
                    new() { name = "tablets" },
                    new() { name = "tops" },
                    new() { name = "vehicle" },
                    new() { name = "womens-bags" },
                    new() { name = "womens-dresses" },
                    new() { name = "womens-jewellery" },
                    new() { name = "womens-shoes" },
                    new() { name = "womens-watches" }
                };
                await context.ProductCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedBrands(AppDbContext context)
        {
            // Solo actuamos si la tabla está vacía
            if (!await context.ProductBrands.AnyAsync())
            {
                var brandNames = new List<string>
        {
            // Tecnología y Smartphones
            "Apple", "Samsung", "Huawei", "Oppo", "Vivo", "Microsoft Surface", "HP Pavilion",
            "Infinix", "Iphone", "OnePlus 12", "Xiaomi", "Motorola", "Sony", "Asus", "Realme",
            "Nothing", "Honor", "Redmi", "Poco", "Tecno", "ZTE", "Meizu", "Lenovo", "Razer",
            "Nokia", "Fairphone", "Gigabyte", "HP", "Microsoft", "Dell", "Acer", "Alienware",
            "LG", "Fujitsu", "Panasonic", "Dynabook", "System76", "Framework", "Purism",

            // Belleza, Perfumes y Cuidado Personal
            "L'Oreal Paris", "Essence", "Glamour Beauty", "Velvet Touch", "Chanel", "Dior",
            "Gucci", "Versace", "Armani Code", "Bvlgari", "Hugo Boss", "Jean Paul Gaultier",
            "Paco Rabanne", "Prada", "Ralph Lauren", "Yves Saint Laurent",

            // Relojería y Lujo
            "Rolex", "Casio", "Fossil", "Luxury Watch", "Garmin", "Fitbit", "Amazfit",
            "Omega", "Tissot", "Seiko", "Longines", "IWC", "Breitling", "Cartier",
            "Audemars", "Patek Philippe Calatrava", "Daniel Wellington", "Citizen",
            "Bulova", "Hamilton", "Movado", "Tudor", "Panerai", "Zenith",
            "Jaeger-LeCoultre", "Girard-Perregaux", "Vacheron Constantin", "Piaget",
            "Chopard", "Blancpain", "Breguet", "Glashutte",

            // Hogar, Muebles y Decoración
            "Furniture Co.", "Knoll", "Bath Trends", "Home Decor", "Annibale Colombo",
            "Blue & Black",

            // Moda y Otros
            "Fashion Trends", "Calvin Klein", "Nike", "Adidas", "Puma", "Nescafe", "Generic", "Luxury"
        };

                // Eliminamos duplicados por si acaso y convertimos a objetos ProductBrand
                var brandsToInsert = brandNames
                    .Distinct()
                    .Select(name => new ProductBrand { name = name })
                    .ToList();

                await context.ProductBrands.AddRangeAsync(brandsToInsert);
                await context.SaveChangesAsync();
            }
        }
    }
}