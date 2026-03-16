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
            reader.Read(); // Salta la cabecera (Fila 1)

            while (reader.Read())
            {
                // El Título es la primera columna (índice 0)
                var title = reader.GetValue(0)?.ToString()?.Trim();

                // Validación de existencia usando el repositorio (asegúrate de aplicar .ToLower() en el Repo también)
                if (string.IsNullOrEmpty(title) || await productRepo.ExistProduct(title)) continue;

                // --- ÍNDICES SEGÚN TU EXCEL ACTUAL ---
                int catId = Convert.ToInt32(reader.GetValue(17)); // Columna R
                int brId = Convert.ToInt32(reader.GetValue(18));  // Columna S

                // Validaciones de existencia de IDs en la DB
                if (!await context.ProductCategories.AnyAsync(c => c.id == catId))
                    throw new Exception($"Error: La CategoryId {catId} no existe.");

                if (!await context.ProductBrands.AnyAsync(b => b.id == brId))
                    throw new Exception($"Error: La BrandId {brId} no existe.");

                var product = new Product
                {
                    title = title,                                                        // Columna A 
                    description = reader.GetValue(1)?.ToString()?.Trim() ?? string.Empty, // Columna B
                    price = Convert.ToDecimal(reader.GetValue(2)),                       // Columna C
                    discountPercentage = Convert.ToDecimal(reader.GetValue(3)),          // Columna D
                    rating = Convert.ToDecimal(reader.GetValue(4)),                      // Columna E
                    stock = Convert.ToInt32(reader.GetValue(5)),                        // Columna F
                    sku = reader.GetValue(6)?.ToString()?.Trim() ?? string.Empty,        // Columna G
                    weight = Convert.ToDecimal(reader.GetValue(7)),
                    width = Convert.ToDecimal(reader.GetValue(8)),
                    height = Convert.ToDecimal(reader.GetValue(9)),
                    depth = Convert.ToDecimal(reader.GetValue(10)),
                    warrantyInformation = reader.GetValue(11)?.ToString()?.Trim() ?? string.Empty,
                    shippingInformation = reader.GetValue(12)?.ToString()?.Trim() ?? string.Empty,
                    availabilityStatus = reader.GetValue(13)?.ToString()?.Trim() ?? string.Empty,
                    returnPolicy = reader.GetValue(14)?.ToString()?.Trim() ?? string.Empty,
                    minimumOrderQuantity = Convert.ToInt32(reader.GetValue(15)),
                    thumbnail = reader.GetValue(16)?.ToString()?.Trim() ?? string.Empty, // Columna Q
                    categoryId = catId,
                    brandId = brId
                };

                // Imágenes en Columna T (Índice 19)
                var imagesRaw = reader.GetValue(19)?.ToString();
                if (!string.IsNullOrWhiteSpace(imagesRaw))
                {
                    product.images = imagesRaw.Split(',')
                        .Select(url => new ProductImage { imageUrl = url.Trim() })
                        .ToList();
                }

                // Tags en Columna U (Índice 20)
                var tagsRaw = reader.GetValue(20)?.ToString();
                if (!string.IsNullOrWhiteSpace(tagsRaw))
                {
                    product.tags = tagsRaw.Split(',')
                        .Select(tag => new ProductTag { tagName = tag.Trim() })
                        .ToList();
                }

                productsToSave.Add(product);
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