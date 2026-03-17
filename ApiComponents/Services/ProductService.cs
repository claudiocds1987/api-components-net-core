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

        // Inicialización simplificada de C# 12
        List<Product> productsToSave = [];
        List<string> errorMessages = [];

        using (var stream = file.OpenReadStream())
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            // --- VALIDACIÓN DE CABECERA ---
            reader.Read(); // Lee la Fila 1 (Cabecera)
            var headerTitle = reader.GetValue(0)?.ToString()?.Trim().ToLower();

            // Verificamos que la primera columna sea 'title'. 
            if (headerTitle != "title")
            {
                throw new Exception("Formato de archivo inválido. La primera columna debe ser 'title'.");
            }

            int rowNumber = 2; // Para rastrear errores por fila

            while (reader.Read())
            {
                try
                {
                    // El Título es la primera columna (índice 0)
                    var title = reader.GetValue(0)?.ToString()?.Trim();

                    // Validación de existencia o vacío
                    if (string.IsNullOrEmpty(title))
                    {
                        rowNumber++;
                        continue;
                    }

                    if (await productRepo.ExistProduct(title))
                    {
                        errorMessages.Add($"Fila {rowNumber}: El producto '{title}' ya existe y fue saltado.");
                        rowNumber++;
                        continue;
                    }

                    // --- ÍNDICES SEGÚN TU EXCEL ACTUAL ---
                    int catId = Convert.ToInt32(reader.GetValue(17)); // Columna R
                    int brId = Convert.ToInt32(reader.GetValue(18));  // Columna S

                    // Validaciones de existencia de IDs en la DB (Acumulando errores)
                    bool categoryExists = await context.ProductCategories.AnyAsync(c => c.id == catId);
                    bool brandExists = await context.ProductBrands.AnyAsync(b => b.id == brId);

                    if (!categoryExists)
                        errorMessages.Add($"Fila {rowNumber}: La CategoryId {catId} no existe.");

                    if (!brandExists)
                        errorMessages.Add($"Fila {rowNumber}: La BrandId {brId} no existe.");

                    // Si hubo errores de integridad en esta fila, saltamos a la siguiente sin crear el objeto
                    if (!categoryExists || !brandExists)
                    {
                        rowNumber++;
                        continue;
                    }

                    var product = new Product
                    {
                        title = title,                                                        // Columna A 
                        description = reader.GetValue(1)?.ToString()?.Trim() ?? string.Empty, // Columna B
                        price = Convert.ToDecimal(reader.GetValue(2)),                        // Columna C
                        discountPercentage = Convert.ToDecimal(reader.GetValue(3)),           // Columna D
                        rating = Convert.ToDecimal(reader.GetValue(4)),                       // Columna E
                        stock = Convert.ToInt32(reader.GetValue(5)),                         // Columna F
                        sku = reader.GetValue(6)?.ToString()?.Trim() ?? string.Empty,         // Columna G
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

                    // Imágenes en Columna T (Índice 19) - Uso de spread operator [.. ]
                    var imagesRaw = reader.GetValue(19)?.ToString();
                    if (!string.IsNullOrWhiteSpace(imagesRaw))
                    {
                        product.images = [.. imagesRaw.Split(',').Select(url => new ProductImage { imageUrl = url.Trim() })];
                    }

                    // Tags en Columna U (Índice 20)
                    var tagsRaw = reader.GetValue(20)?.ToString();
                    if (!string.IsNullOrWhiteSpace(tagsRaw))
                    {
                        product.tags = [.. tagsRaw.Split(',').Select(tag => new ProductTag { tagName = tag.Trim() })];
                    }

                    productsToSave.Add(product);
                    rowNumber++;
                }
                catch (Exception ex)
                {
                    // Captura errores de formato (letras en campos de números, etc.)
                    errorMessages.Add($"Fila {rowNumber}: Error de formato o dato inválido ({ex.Message})");
                    rowNumber++;
                }
            }
        }

        // Si al finalizar el bucle hay mensajes de error en la lista, lanzamos la excepción con todos los errores juntos
        if (errorMessages.Count > 0)
        {
            throw new Exception(string.Join("\n", errorMessages));
        }

        if (productsToSave.Count > 0)
        {
            await productRepo.AddProductsList(productsToSave);
        }
    }

    public async Task<Product> GetProductByIdAsync(int id) => await productRepo.GetProduct(id);

    public async Task<object> GetAllProductsAsync(int? page, int? size)
    {
        var (items, totalCount) = await productRepo.GetProductsAsync(page, size);

        return new
        {
            Items = items,
            TotalItems = totalCount,
            PageNumber = page ?? 1,
            PageSize = size ?? totalCount,
            TotalPages = size.HasValue ? (int)Math.Ceiling(totalCount / (double)size.Value) : 1
        };
    }

    public async Task UpdateProductAsync(Product product) => await productRepo.UpdateProduct(product);
    public async Task DeleteProductAsync(int id) => await productRepo.DeleteProduct(id);
}