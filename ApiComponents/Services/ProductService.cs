using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using ApiComponents.Persistence.Repositories;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Services;

public class ProductService(IProductRepository productRepo, AppDbContext context) : IProductService
{
    public async Task ProcessExcelAsync(IFormFile file)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var productsToSave = new List<Product>();

        using (var stream = file.OpenReadStream())
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            reader.Read(); // Saltamos cabecera

            while (reader.Read())
            {
                var title = reader.GetValue(1)?.ToString()?.Trim();
                if (string.IsNullOrEmpty(title) || await productRepo.ExistProduct(title)) continue;

                // 1. Extraemos los IDs directamente del Excel
                int catId = Convert.ToInt32(reader.GetValue(18)); // Columna S
                int brId = Convert.ToInt32(reader.GetValue(19));  // Columna T

                // 2. VALIDACIÓN PROFESIONAL: ¿Existen en mi base de datos?
                var categoryExists = await context.ProductCategories.AnyAsync(c => c.id == catId);
                var brandExists = await context.ProductBrands.AnyAsync(b => b.id == brId);

                if (!categoryExists)
                    throw new Exception($"Error en producto '{title}': La CategoryId {catId} no existe.");

                if (!brandExists)
                    throw new Exception($"Error en producto '{title}': La BrandId {brId} no existe.");

                // 3. Si todo está bien, lo agregamos a la lista
                productsToSave.Add(new Product
                {
                    title = reader.GetValue(1)?.ToString()?.Trim() ?? string.Empty,
                    description = reader.GetValue(2)?.ToString()?.Trim() ?? string.Empty,
                    price = Convert.ToDecimal(reader.GetValue(3)),
                    discountPercentage = Convert.ToDecimal(reader.GetValue(4)),
                    rating = Convert.ToDecimal(reader.GetValue(5)),
                    stock = Convert.ToInt32(reader.GetValue(6)),
                    sku = reader.GetValue(7)?.ToString()?.Trim() ?? string.Empty,
                    weight = Convert.ToDecimal(reader.GetValue(8)),
                    width = Convert.ToDecimal(reader.GetValue(9)),
                    height = Convert.ToDecimal(reader.GetValue(10)),
                    depth = Convert.ToDecimal(reader.GetValue(11)),
                    warrantyInformation = reader.GetValue(12)?.ToString()?.Trim() ?? string.Empty,
                    shippingInformation = reader.GetValue(13)?.ToString()?.Trim() ?? string.Empty,
                    availabilityStatus = reader.GetValue(14)?.ToString()?.Trim() ?? string.Empty,
                    returnPolicy = reader.GetValue(15)?.ToString()?.Trim() ?? string.Empty,
                    minimumOrderQuantity = Convert.ToInt32(reader.GetValue(16)),
                    thumbnail = reader.GetValue(17)?.ToString()?.Trim() ?? string.Empty,

                    // IDs estrictos que validaste previamente
                    categoryId = catId, // Índice 18
                    brandId = brId      // Índice 19
                });
            }
        }

        if (productsToSave.Count > 0)
        {
            await productRepo.AddProductsList(productsToSave);
        }
    }

    public async Task<Product> GetProductByIdAsync(int id) => await productRepo.GetProduct(id);
    public async Task UpdateProductAsync(Product product) => await productRepo.UpdateProduct(product);
    public async Task DeleteProductAsync(int id) => await productRepo.DeleteProduct(id);
}