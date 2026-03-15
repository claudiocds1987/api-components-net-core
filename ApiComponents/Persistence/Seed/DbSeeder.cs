using ApiComponents.Models;
using ApiComponents.Persistence.Context;

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
            if (!context.ProductBrands.Any())
            {
                var brands = new List<ProductBrand>
        {
            // Tecnología / Smartphones / Laptops
            new() { name = "Apple" },
            new() { name = "Samsung" },
            new() { name = "Huawei" },
            new() { name = "Oppo" },
            new() { name = "Vivo" },
            new() { name = "Microsoft Surface" },
            new() { name = "HP Pavilion" },
            new() { name = "Infinix" },
            
            // Belleza / Fragancias
            new() { name = "L'Oreal Paris" },
            new() { name = "Essence" },
            new() { name = "Glamour Beauty" },
            new() { name = "Velvet Touch" },
            new() { name = "Chanel" },
            new() { name = "Dior" },
            new() { name = "Gucci" },
            
            // Hogar / Muebles / Decoración
            new() { name = "Furniture Co." },
            new() { name = "Knoll" },
            new() { name = "Bath Trends" },
            new() { name = "Home Decor" },
            
            // Relojes / Joyas / Accesorios
            new() { name = "Rolex" },
            new() { name = "Casio" },
            new() { name = "Fossil" },
            new() { name = "Luxury Watch" },
            new() { name = "Fashion Trends" },
            
            // Otros / Supermercado
            new() { name = "Annibale Colombo" },
            new() { name = "Calvin Klein" },
            new() { name = "Nike" },
            new() { name = "Adidas" },
            new() { name = "Puma" }
        };

                await context.ProductBrands.AddRangeAsync(brands);
                await context.SaveChangesAsync();
            }
        }
    }
}