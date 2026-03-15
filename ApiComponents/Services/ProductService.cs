using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using ApiComponents.Persistence.Repositories;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Services
{
    // Usamos Constructor Principal (Sugerencia IDE0290)
    public class ProductService(IProductRepository productRepo, AppDbContext context) : IProductService
    {
        public async Task ProcessExcelAsync(IFormFile file)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var productsToSave = new List<Product>();

            using (var stream = file.OpenReadStream())
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                reader.Read(); // Cabecera

                while (reader.Read())
                {
                    var title = reader.GetValue(1)?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(title) || await productRepo.ExistProduct(title)) continue;

                    var rawBrandName = reader.GetValue(10)?.ToString()?.Trim();
                    var brandName = string.IsNullOrEmpty(rawBrandName) ? "Generic" : rawBrandName;

                    // Comparación optimizada (Sugerencia CA1862)
                    var brand = await context.ProductBrands
                        .FirstOrDefaultAsync(b => string.Equals(b.name, brandName, StringComparison.OrdinalIgnoreCase));

                    if (brand == null)
                    {
                        brand = new ProductBrand { name = brandName };
                        context.ProductBrands.Add(brand);
                        await context.SaveChangesAsync();
                    }

                    productsToSave.Add(new Product
                    {
                        title = title,
                        description = reader.GetValue(2)?.ToString()?.Trim() ?? string.Empty,
                        categoryId = Convert.ToInt32(reader.GetValue(3)),
                        brandId = brand.id,
                        price = Convert.ToDecimal(reader.GetValue(4)),
                        stock = Convert.ToInt32(reader.GetValue(7)),
                        sku = reader.GetValue(8)?.ToString()?.Trim() ?? string.Empty,
                        thumbnail = reader.GetValue(20)?.ToString()?.Trim() ?? string.Empty
                    });
                }
            }

            if (productsToSave.Count > 0) // Sugerencia CA1860: Count > 0 es más rápido que Any() en Listas
            {
                await productRepo.AddProductsList(productsToSave);
            }
        }

        public async Task<Product> GetProductByIdAsync(int id) => await productRepo.GetProduct(id);

        public async Task UpdateProductAsync(Product product) => await productRepo.UpdateProduct(product);

        public async Task DeleteProductAsync(int id) => await productRepo.DeleteProduct(id);
    }
}