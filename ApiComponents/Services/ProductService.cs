using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using ApiComponents.Persistence.Repositories;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ApiComponents.Services;

public class ProductService(IProductRepository productRepo, AppDbContext context) : IProductService
{

    public async Task<ImportResultDto> ProcessCsvAsync(IFormFile file)
    {
        var result = new ImportResultDto();
        List<Product> productsToSave = [];

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";", // Delimitador de Columnas/campos del archivo .csv
                             // Para que no importe si hay espacios después del punto y coma
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.ToLower().Trim(),
            // IMPORTANTE: Si un campo falla, que no explote toda la lectura, sino que lo atrape el catch interno
            MissingFieldFound = null,
            HeaderValidated = null
        };

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, config);

            // Leer encabezados
            await csv.ReadAsync();
            csv.ReadHeader();

            // VALIDACIÓN DE CABECERA (mantenemos tu lógica)
            if (!csv.HeaderRecord[0].Contains("title"))
            {
                result.Success = false;
                result.Message = "Formato inválido. La primera columna debe ser 'title'.";
                return result;
            }

            int rowNumber = 2; // La fila 1 son los encabezados

            while (await csv.ReadAsync())
            {
                try
                {
                    var title = csv.GetField("title")?.Trim();

                    if (string.IsNullOrEmpty(title)) { rowNumber++; continue; }

                    // 1. Validar si ya existe
                    if (await productRepo.ExistProduct(title))
                    {
                        result.Errors.Add($"Fila {rowNumber}: El producto '{title}' ya existe y fue saltado.");
                        rowNumber++; continue;
                    }

                    // 2. Validar Categoría y Marca (Blindado con TryParse)
                    var catField = csv.GetField("categoryid");
                    var brandField = csv.GetField("brandid");

                    if (!int.TryParse(catField, out int catId))
                    {
                        result.Errors.Add($"Fila {rowNumber}: El valor de CategoryId '{catField}' no es un número válido.");
                        rowNumber++; continue;
                    }

                    if (!int.TryParse(brandField, out int brId))
                    {
                        result.Errors.Add($"Fila {rowNumber}: El valor de BrandId '{brandField}' no es un número válido.");
                        rowNumber++; continue;
                    }

                    bool categoryExists = await context.ProductCategories.AnyAsync(c => c.id == catId);
                    bool brandExists = await context.ProductBrands.AnyAsync(b => b.id == brId);

                    if (!categoryExists) result.Errors.Add($"Fila {rowNumber}: La CategoryId {catId} no existe en la base de datos.");
                    if (!brandExists) result.Errors.Add($"Fila {rowNumber}: La BrandId {brId} no existe en la base de datos.");

                    if (!categoryExists || !brandExists) { rowNumber++; continue; }

                    // 3. Mapeo del objeto
                    var product = new Product
                    {
                        title = title,
                        description = csv.GetField("description") ?? string.Empty,

                        // Mapeo de Números (Decimales con Punto)
                        price = decimal.Parse(csv.GetField("price") ?? "0", CultureInfo.InvariantCulture),
                        discountPercentage = decimal.Parse(csv.GetField("discountpercentage") ?? "0", CultureInfo.InvariantCulture),
                        rating = decimal.Parse(csv.GetField("rating") ?? "0", CultureInfo.InvariantCulture),

                        // Mapeo de Enteros
                        stock = int.Parse(csv.GetField("stock") ?? "0"),
                        minimumOrderQuantity = int.Parse(csv.GetField("minimumorderquantity") ?? "0"),

                        // Mapeo de Strings
                        sku = csv.GetField("sku") ?? string.Empty,
                        warrantyInformation = csv.GetField("warrantyinformation") ?? string.Empty,
                        shippingInformation = csv.GetField("shippinginformation") ?? string.Empty,
                        availabilityStatus = csv.GetField("availabilitystatus") ?? string.Empty,
                        returnPolicy = csv.GetField("returnpolicy") ?? string.Empty,
                        thumbnail = csv.GetField("thumbnail") ?? string.Empty,

                        // Dimensiones (Decimales)
                        weight = decimal.Parse(csv.GetField("weight") ?? "0", CultureInfo.InvariantCulture),
                        width = decimal.Parse(csv.GetField("width") ?? "0", CultureInfo.InvariantCulture),
                        height = decimal.Parse(csv.GetField("height") ?? "0", CultureInfo.InvariantCulture),
                        depth = decimal.Parse(csv.GetField("depth") ?? "0", CultureInfo.InvariantCulture),

                        // Relaciones (Ya validadas antes en tu código)
                        categoryId = catId,
                        brandId = brId
                    };

                    // Imágenes (Columna T / images)
                    var imagesRaw = csv.GetField("images");
                    if (!string.IsNullOrWhiteSpace(imagesRaw))
                    {
                        product.images = [.. imagesRaw.Split(',').Select(url => new ProductImage { imageUrl = url.Trim() })];
                    }

                    // Tags (Columna U / tags)
                    var tagsRaw = csv.GetField("tags");
                    if (!string.IsNullOrWhiteSpace(tagsRaw))
                    {
                        product.tags = [.. tagsRaw.Split(',').Select(tag => new ProductTag { tagName = tag.Trim() })];
                    }

                    productsToSave.Add(product);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Fila {rowNumber}: Error de datos detallado ({ex.Message})");
                }
                rowNumber++;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            // si el error es de CsvHelper, va a decir qué columna o fila falló
            result.Message = $"Error crítico: {ex.Message}";
            result.Errors.Add($"DETALLE TÉCNICO: {ex.Message}");

            if (ex.InnerException != null)
                result.Errors.Add($"CAUSA RAÍZ: {ex.InnerException.Message}");

            return result;
        }

        // --- FINALIZACIÓN ---
        if (result.Errors.Count > 0)
        {
            result.Success = false;
            // El mensaje va a decir cuántos errores hubo para que el usuario sepa que falló
            result.Message = $"Se encontraron {result.Errors.Count} errores en las filas. Revise el reporte para más detalle.";
            return result;
        }

        if (productsToSave.Count > 0)
        {
            await productRepo.AddProductsList(productsToSave);
            result.Success = true;
            result.Message = "Productos cargados exitosamente.";
            result.Count = productsToSave.Count;
        }

        return result;
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