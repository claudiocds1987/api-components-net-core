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
                    new() { name = "womens-watches" },
                    new() { name = "smart-tv" }
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
            "Apple", "Samsung", "Huawei", "Oppo", "Vivo", "Microsoft Surface",
            "HP Pavilion", "Infinix", "L'Oreal Paris", "Essence", "Glamour Beauty",
            "Velvet Touch", "Chanel", "Dior", "Gucci", "Furniture Co", "Knoll",
            "Bath Trends", "Home Decor", "Rolex", "Casio", "Fossil", "Luxury Watch",
            "Fashion Trends", "Annibale Colombo", "Calvin Klein", "Nike", "Adidas",
            "Puma", "Nescafe", "Versace", "Armani Code", "Bvlgari", "Hugo Boss",
            "Jean Paul Gaultier", "Paco Rabanne", "Prada", "Ralph Lauren",
            "Yves Saint Laurent", "Iphone", "OnePlus", "Xiaomi", "Motorola",
            "Sony", "Asus", "Realme", "Nothing", "Honor", "Redmi", "Poco",
            "Tecno", "ZTE", "Meizu", "Lenovo", "Razer", "Nokia", "Fairphone",
            "Generic", "Luxury", "Garmin", "Fitbit", "Amazfit", "Omega",
            "Tissot", "Seiko", "Longines", "IWC", "Breitling", "Cartier",
            "Audemars", "Patek Philippe Calatrava", "Daniel Wellington", "Citizen",
            "Bulova", "Hamilton", "Movado", "Tudor", "Panerai", "Zenith",
            "Jaeger-LeCoultre", "Girard-Perregaux", "Vacheron Constantin",
            "Piaget", "Chopard", "Blancpain", "Breguet", "Glashutte",
            "Blue & Black", "Gigabyte", "HP", "Microsoft", "Dell", "Acer",
            "Alienware", "LG", "Fujitsu", "Panasonic", "Dynabook", "System76",
            "Framework", "Purism", "MSI", "Hublot", "MVMT", "TAG Heuer",
            "Google", "Dolce & Gabbana", "Nail Couture", "Chic Cosmetics"
        };

                // Distinct para evitar errores de índice único y Select para crear los objetos
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