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
        var result = new ImportResultDto(); // Objeto para devolver el resultado (éxito/error y mensajes)
        List<Product> productsToSave = []; // Lista temporal para acumular los productos válidos antes de guardarlos

        // Configuración del lector de CSV
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true, // Indica que la primera fila contiene los nombres de las columnas
            Delimiter = ";", // Define el punto y coma como separador de campos
            TrimOptions = TrimOptions.Trim, // Elimina espacios en blanco accidentales al inicio/final de cada celda
            PrepareHeaderForMatch = args => args.Header.ToLower().Trim(), // Convierte encabezados a minúsculas para que no falle por mayúsculas
            MissingFieldFound = null, // Evita que el programa explote si falta una columna en alguna fila
            HeaderValidated = null // Desactiva la validación estricta de encabezados para mayor flexibilidad
        };

        try
        {
            // Abre el flujo de lectura del archivo subido
            using var reader = new StreamReader(file.OpenReadStream());
            // Inicializa CsvHelper con el lector y la configuración definida
            using var csv = new CsvReader(reader, config);

            // Lee la primera fila (los encabezados) de forma asíncrona
            await csv.ReadAsync();
            csv.ReadHeader();

            // Valida estrictamente que la primera columna física (índice 0) sea el título, Para las demas columnas no importa el orden.
            if (!csv.HeaderRecord[0].Contains("title"))
            {
                result.Success = false; // Marca la operación como fallida
                result.Message = "Formato inválido. La primera columna debe ser 'title'.";
                return result; // Corta la ejecución y devuelve el error
            }

            int rowNumber = 2; // Contador de filas para reportar errores (empezamos en 2 porque la 1 es el encabezado)

            // Itera mientras haya filas de datos para leer
            while (await csv.ReadAsync())
            {
                try
                {
                    // Obtiene el valor de la columna "title" buscando por nombre de encabezado
                    var title = csv.GetField("title")?.Trim();

                    // Si el título está vacío, ignora la fila y pasa a la siguiente
                    if (string.IsNullOrEmpty(title)) { rowNumber++; continue; }

                    // Verifica en la base de datos si ya existe un producto con ese mismo nombre
                    if (await productRepo.ExistProduct(title))
                    {
                        result.Errors.Add($"Fila {rowNumber}: El producto '{title}' ya existe y fue saltado.");
                        rowNumber++; continue;
                    }

                    // Extrae los valores de categoría y marca como texto
                    var catField = csv.GetField("categoryid");
                    var brandField = csv.GetField("brandid");

                    // Intenta convertir el texto de categoría a un número entero
                    if (!int.TryParse(catField, out int catId))
                    {
                        result.Errors.Add($"Fila {rowNumber}: El valor de CategoryId '{catField}' no es un número válido.");
                        rowNumber++; continue;
                    }

                    // Intenta convertir el texto de marca a un número entero
                    if (!int.TryParse(brandField, out int brId))
                    {
                        result.Errors.Add($"Fila {rowNumber}: El valor de BrandId '{brandField}' no es un número válido.");
                        rowNumber++; continue;
                    }

                    // Verifica si el ID de categoría existe realmente en la tabla ProductCategories
                    bool categoryExists = await context.ProductCategories.AnyAsync(c => c.id == catId);
                    // Verifica si el ID de marca existe realmente en la tabla ProductBrands
                    bool brandExists = await context.ProductBrands.AnyAsync(b => b.id == brId);

                    // Si no existen, agrega el error correspondiente a la lista de errores
                    if (!categoryExists) result.Errors.Add($"Fila {rowNumber}: La CategoryId {catId} no existe en la base de datos.");
                    if (!brandExists) result.Errors.Add($"Fila {rowNumber}: La BrandId {brId} no existe en la base de datos.");

                    // Si alguno de los dos falla, salta la fila
                    if (!categoryExists || !brandExists) { rowNumber++; continue; }

                    // Crea la instancia del objeto Producto y mapea los campos del CSV a las propiedades
                    var product = new Product
                    {
                        title = title,
                        description = csv.GetField("description") ?? string.Empty, // Si es nulo, pone texto vacío

                        // Parsea números decimales usando punto como separador decimal (InvariantCulture)
                        price = decimal.Parse(csv.GetField("price") ?? "0", CultureInfo.InvariantCulture),
                        discountPercentage = decimal.Parse(csv.GetField("discountpercentage") ?? "0", CultureInfo.InvariantCulture),
                        rating = decimal.Parse(csv.GetField("rating") ?? "0", CultureInfo.InvariantCulture),

                        // Parsea números enteros (Stock y Cantidad mínima)
                        stock = int.Parse(csv.GetField("stock") ?? "0"),
                        minimumOrderQuantity = int.Parse(csv.GetField("minimumorderquantity") ?? "0"),

                        // Asignación de cadenas de texto simples
                        sku = csv.GetField("sku") ?? string.Empty,
                        warrantyInformation = csv.GetField("warrantyinformation") ?? string.Empty,
                        shippingInformation = csv.GetField("shippinginformation") ?? string.Empty,
                        availabilityStatus = csv.GetField("availabilitystatus") ?? string.Empty,
                        returnPolicy = csv.GetField("returnpolicy") ?? string.Empty,
                        thumbnail = csv.GetField("thumbnail") ?? string.Empty,

                        // Parsea números decimales usando punto como separador decimal (InvariantCulture)
                        weight = decimal.Parse(csv.GetField("weight") ?? "0", CultureInfo.InvariantCulture),
                        width = decimal.Parse(csv.GetField("width") ?? "0", CultureInfo.InvariantCulture),
                        height = decimal.Parse(csv.GetField("height") ?? "0", CultureInfo.InvariantCulture),
                        depth = decimal.Parse(csv.GetField("depth") ?? "0", CultureInfo.InvariantCulture),

                        // Asigna los IDs de las llaves foráneas ya validados
                        categoryId = catId,
                        brandId = brId
                    };

                    // Procesa la columna de imágenes (asume que están separadas por comas dentro de la celda)
                    var imagesRaw = csv.GetField("images");
                    if (!string.IsNullOrWhiteSpace(imagesRaw))
                    {
                        // Divide el texto por comas y crea una lista de objetos ProductImage
                        product.images = [.. imagesRaw.Split(',').Select(url => new ProductImage { imageUrl = url.Trim() })];
                    }

                    // Procesa la columna de etiquetas (tags) separadas por comas
                    var tagsRaw = csv.GetField("tags");
                    if (!string.IsNullOrWhiteSpace(tagsRaw))
                    {
                        // Divide el texto y crea una lista de objetos ProductTag
                        product.tags = [.. tagsRaw.Split(',').Select(tag => new ProductTag { tagName = tag.Trim() })];
                    }

                    // Si todo salió bien, agrega el producto a la lista para guardar
                    productsToSave.Add(product);
                }
                catch (Exception ex)
                {
                    // Si ocurre un error inesperado en una fila específica, lo captura y lo agrega a la lista de errores con el número de fila para referencia.
                    result.Errors.Add($"Fila {rowNumber}: Error de datos detallado ({ex.Message})");
                }
                rowNumber++; // Incrementa el contador de filas para la siguiente vuelta
            }
        }
        catch (Exception ex)
        {
            // Captura errores críticos (archivo corrupto, falta de memoria, etc.)
            result.Success = false;
            result.Message = $"Error crítico: {ex.Message}";
            result.Errors.Add($"DETALLE TÉCNICO: {ex.Message}");

            // Si hay un error interno más profundo, también lo informa
            if (ex.InnerException != null)
                result.Errors.Add($"CAUSA RAÍZ: {ex.InnerException.Message}");

            return result;
        }

        // Si hubo errores durante la lectura de las filas, devuelve el reporte de errores
        if (result.Errors.Count > 0)
        {
            result.Success = false;
            result.Message = $"Se encontraron {result.Errors.Count} errores en las filas. Revise el reporte para más detalle.";
            return result;
        }

        // Si hay productos válidos, los guarda todos de una sola vez en la base de datos
        if (productsToSave.Count > 0)
        {
            await productRepo.AddProductsList(productsToSave);
            result.Success = true;
            result.Message = "Productos cargados exitosamente.";
            result.Count = productsToSave.Count; // Informa cuántos productos se crearon
        }

        return result; // Devuelve el resultado final al controlador
    }
    public async Task<Product?> GetProductByIdAsync(int id) => await productRepo.GetProduct(id);

    public async Task<object> GetAllProductsAsync(int? page, int? size)
    {
        var (items, totalCount) = await productRepo.GetProductsAsync(page, size);

        return new
        {
            items = items,
            totalItems = totalCount,
            pageNumber = page ?? 1,
            pageSize = size ?? totalCount,
            totalPages = size.HasValue ? (int)Math.Ceiling(totalCount / (double)size.Value) : 1
        };
    }

    public async Task UpdateProductAsync(Product product) => await productRepo.UpdateProduct(product);
    public async Task DeleteProductAsync(int id) => await productRepo.DeleteProduct(id);
}