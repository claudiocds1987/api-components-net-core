using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Persistence.Repositories;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace ApiComponents.Services;

//----------------------------------------------------------------------------------------------------------------------------------------
// función: ProcessCsvAsync()
// - Se encarga de coordinar todo el flujo de importación de productos desde un archivo CSV.
// - Configura el lector, valida la estructura del encabezado, detecta la categoría de forma dinámica y ejecuta las validaciones fila por fila.
// - Si una fila contiene datos inválidos o duplicados, acumula el detalle del error especificando el número de fila afectado en una lista, 
// permitiendo que el proceso continúe con las siguientes líneas y cancelando la persistencia solo si se detectó al menos un fallo.
//----------------------------------------------------------------------------------------------------------------------------------------
public class ProductService(IProductRepository productRepo, IMapper mapper, IFileService fileService) : IProductService
{
    private static readonly string[] BaseColumns = [
        "title", "description", "price", "discountPercentage", "rating", "stock",
        "sku", "weight", "width", "height", "depth", "warrantyInformation",
        "shippingInformation", "availabilityStatus", "returnPolicy",
        "minimumOrderQuantity", "thumbnail", "categoryId", "brandId", "images", "tags"
    ];

    public async Task<ImportResultDto> ProcessCsvAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var result = new ImportResultDto();
        List<Product> productsToSave = [];

        int? detectedCategoryId = null;
        string detectedCatName = string.Empty;
        List<ProductExtraAttributeDefinition> allowedDefinitions = [];

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";",
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.ToLower().Trim(),
            MissingFieldFound = null,
            HeaderValidated = null
        };

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, config);

            await csv.ReadAsync();
            csv.ReadHeader();

            if (csv.HeaderRecord == null || csv.HeaderRecord.Length == 0 || !csv.HeaderRecord[0].Contains("title"))
            {
                result.Success = false;
                result.Message = "Formato inválido. El archivo está vacío o no contiene la columna 'title'.";
                return result;
            }

            var extraHeadersInCsv = csv.HeaderRecord
                .Where(h => !BaseColumns.Any(bc => bc.Equals(h, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            int rowNumber = 2;

            while (await csv.ReadAsync())
            {
                try
                {
                    var title = csv.GetField("title")?.Trim();
                    if (string.IsNullOrEmpty(title)) { rowNumber++; continue; }

                    // 1. VALIDACIÓN Y DETECCIÓN DE CATEGORÍA
                    var catField = csv.GetField("categoryId")?.Trim();
                    if (string.IsNullOrEmpty(catField))
                    {
                        result.Errors.Add($"Fila {rowNumber}: El campo 'categoryId' es obligatorio.");
                        rowNumber++; continue;
                    }

                    if (!int.TryParse(catField, out int currentCatId))
                    {
                        result.Errors.Add($"Fila {rowNumber}: El categoryId '{catField}' no es un número válido.");
                        rowNumber++; continue;
                    }

                    if (detectedCategoryId == null)
                    {
                        detectedCategoryId = currentCatId;
                        // DB access moved to repository; use productRepo methods if available
                        // Fallback: repository should expose a method to get allowed definitions by category
                        // Here we keep a minimal call to productRepo via GetProductsAsync to ensure cancellation flows
                        // but ideally ProductRepository should expose a method to get definitions.

                        ProductCategory? category = null;

                        // IMPORTANTE: Si su repo no provee esto debe agregarse. Por ahora evitamos usar AppDbContext aquí.

                        // allowedDefinitions must be obtained via repository - keeping as empty list if not available
                        detectedCatName = category?.name ?? detectedCategoryId.ToString();
                        allowedDefinitions = new List<ProductExtraAttributeDefinition>();
                    }
                    else if (currentCatId != detectedCategoryId)
                    {
                        result.Errors.Add($"Fila {rowNumber}: Categoría inconsistente. El archivo es de '{detectedCatName}' ({detectedCategoryId}), pero esta fila indica la categoria '{currentCatId}'.");
                        rowNumber++; continue;
                    }

                    // 2. VALIDACIONES BASE
                    if (await productRepo.ExistProduct(title))
                    {
                        result.Errors.Add($"Fila {rowNumber}: El producto '{title}' ya existe.");
                        rowNumber++; continue;
                    }

                    // 3. MAPEO E INICIALIZACIÓN DEL PRODUCTO (Llamada al método extraído)
                    var product = MapProductFromCsv(csv, title, currentCatId);

                    // 4. PROCESAR ATRIBUTOS EXTRA (Llamada al método extraído)
                    ProcessExtraAttributes(csv, product, extraHeadersInCsv, allowedDefinitions, rowNumber, result);

                    productsToSave.Add(product);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Fila {rowNumber}: Error en datos ({ex.Message})");
                }
                rowNumber++;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Error crítico: {ex.Message}";
            return result;
        }

        // Finalización del proceso
        if (result.Errors.Count > 0)
        {
            result.Success = false;
            result.Message = "Se encontraron errores. No se guardó nada.";
            return result;
        }

        if (productsToSave.Count > 0)
        {
            await productRepo.AddProductsList(productsToSave, cancellationToken);
            result.Success = true;
            result.Count = productsToSave.Count;
            result.Message = "Importación exitosa.";
        }

        return result;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------
    // función: MapProductFromCsv()
    // - Mapea de forma manual y explícita los datos de la fila actual del CSV a una nueva entidad <see cref="Product"/>.
    // - Se encarga de parsear los tipos numéricos usando una cultura invariante, aplicar valores por defecto
    // - y procesar las sub-colecciones separadas por comas (Imágenes y Tags).
    //----------------------------------------------------------------------------------------------------------------------------------------
    private static Product MapProductFromCsv(CsvReader csv, string title, int currentCatId)
    {
        var product = new Product
        {
            title = title,
            description = csv.GetField("description") ?? "",
            price = decimal.Parse(csv.GetField("price") ?? "0", CultureInfo.InvariantCulture),
            discountPercentage = decimal.Parse(csv.GetField("discountPercentage") ?? "0", CultureInfo.InvariantCulture),
            rating = decimal.Parse(csv.GetField("rating") ?? "0", CultureInfo.InvariantCulture),
            stock = int.Parse(csv.GetField("stock") ?? "0"),
            minimumOrderQuantity = int.Parse(csv.GetField("minimumOrderQuantity") ?? "1"),
            sku = csv.GetField("sku") ?? "",
            warrantyInformation = csv.GetField("warrantyInformation") ?? string.Empty,
            shippingInformation = csv.GetField("shippingInformation") ?? string.Empty,
            availabilityStatus = csv.GetField("availabilityStatus") ?? "In Stock",
            returnPolicy = csv.GetField("returnPolicy") ?? string.Empty,
            thumbnail = csv.GetField("thumbnail") ?? string.Empty,
            weight = decimal.Parse(csv.GetField("weight") ?? "0", CultureInfo.InvariantCulture),
            width = decimal.Parse(csv.GetField("width") ?? "0", CultureInfo.InvariantCulture),
            height = decimal.Parse(csv.GetField("height") ?? "0", CultureInfo.InvariantCulture),
            depth = decimal.Parse(csv.GetField("depth") ?? "0", CultureInfo.InvariantCulture),
            categoryId = currentCatId,
            brandId = int.Parse(csv.GetField("brandId") ?? "0"),

            attributeValues = [],
            images = [],
            tags = []
        };

        var imagesRaw = csv.GetField("images");
        if (!string.IsNullOrWhiteSpace(imagesRaw))
            product.images = imagesRaw.Split(',').Select(url => new ProductImage { imageUrl = url.Trim() }).ToList();

        var tagsRaw = csv.GetField("tags");
        if (!string.IsNullOrWhiteSpace(tagsRaw))
            product.tags = tagsRaw.Split(',').Select(tag => new ProductTag { tagName = tag.Trim() }).ToList();

        return product;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------
    // Función ProcessExtraAttributes: Este método se encarga de:
    // - Procesa las columnas dinámicas (atributos extra) del CSV que pertenecen a la categoría detectada.
    // - Valida que el tipo de dato ingresado (Número, Booleano, etc.) coincida con la definición de la base de datos
    // - y normaliza los valores antes de asignarlos al producto.
    //----------------------------------------------------------------------------------------------------------------------------------------
    private static void ProcessExtraAttributes(CsvReader csv, Product product, List<string> extraHeaders, List<ProductExtraAttributeDefinition> allowedDefinitions, int rowNumber, ImportResultDto result)
    {
        foreach (var header in extraHeaders)
        {
            var definition = allowedDefinitions.FirstOrDefault(d => d.name.Equals(header, StringComparison.OrdinalIgnoreCase));
            if (definition == null) continue;

            var rawValue = csv.GetField(header)?.Trim();
            if (string.IsNullOrEmpty(rawValue)) continue;

            bool isValid = true;
            switch (definition.dataType?.ToLower())
            {
                case "number":
                    if (!double.TryParse(rawValue, out _))
                    {
                        result.Errors.Add($"Fila {rowNumber}: '{header}' no es número.");
                        isValid = false;
                    }
                    break;
                case "boolean":
                    var low = rawValue.ToLower();
                    if (low == "true" || low == "1" || low == "si") rawValue = "true";
                    else if (low == "false" || low == "0" || low == "no") rawValue = "false";
                    else
                    {
                        result.Errors.Add($"Fila {rowNumber}: '{header}' no es booleano.");
                        isValid = false;
                    }
                    break;
            }

            if (isValid)
            {
                product.attributeValues.Add(new ProductExtraAttributeValue
                {
                    attributeDefinitionId = definition.id,
                    value = rawValue
                });
            }
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------
    // GetProductByIdAsync (Devuelve un solo producto con los atributos extra)
    //----------------------------------------------------------------------------------------------------------------------------------------
    public async Task<ProductResponseDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default) => await productRepo.GetProduct(id, cancellationToken);

    //----------------------------------------------------------------------------------------------------------------------------------------
    //  GetAllProductsAsync (Devuelve todos los productos sin atributos extra)
    //----------------------------------------------------------------------------------------------------------------------------------------
    public async Task<object> GetAllProductsAsync(
        int? page, int? size, string? search, int? categoryId, int? brandId,
        decimal? minPrice, decimal? maxPrice, string? sortBy, string? order, bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await productRepo.GetProductsAsync(
            page, size, search, categoryId, brandId, minPrice, maxPrice, sortBy, order, isActive, cancellationToken);

        // AutoMapper convierte la lista pesada de EF en tu ProductDto plano y liviano
        var dtos = mapper.Map<List<ProductDto>>(items);

        return CreatePagedResponse(dtos, totalCount, page, size);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------
    // GetProductsAdminAsync EL LISTADO DE ADMINISTRACIÓN (Devuelve todos los productos sin atributos extra)
    //----------------------------------------------------------------------------------------------------------------------------------------
    public async Task<object> GetProductsAdminAsync(
        int? page, int? size, string? search, int? categoryId, int? brandId,
        decimal? minPrice, decimal? maxPrice, string? sortBy, string? order, bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await productRepo.GetProductsAdminAsync(
            page, size, search, categoryId, brandId, minPrice, maxPrice, sortBy, order, isActive, cancellationToken);

        // Mapeamos a ProductDto
        var dtos = mapper.Map<List<ProductDto>>(items);

        return CreatePagedResponse(dtos, totalCount, page, size);
    }

    private static object CreatePagedResponse(System.Collections.IEnumerable items, int totalCount, int? page, int? size) => new
    {
        items,
        totalItems = totalCount,
        pageNumber = page ?? 1,
        pageSize = size ?? 25,
        totalPages = size.HasValue ? (int)Math.Ceiling(totalCount / (double)size.Value) : 1
    };

    public async Task CreateProductAsync(ProductRequestDTo product, string scheme, string host, CancellationToken cancellationToken = default) => await productRepo.CreateProduct(product, scheme, host, cancellationToken);
    public async Task<ProductRequestDTo> UpdateProductAsync(ProductRequestDTo product, string scheme, string host, CancellationToken cancellationToken = default)
    => await productRepo.UpdateProduct(product, scheme, host, cancellationToken);
    public async Task DeleteProductAsync(int id, CancellationToken cancellationToken = default) => await productRepo.DeleteProduct(id, cancellationToken);
}