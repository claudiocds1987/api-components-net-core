using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Persistence.Context;
using ApiComponents.Services;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Persistence.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task AddProductsList(List<Product> products)
    {
        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistProduct(string title)
    => await db.Products.AnyAsync(p => EF.Functions.Like(p.title, title));

    // 1. OBTENER UN SOLO PRODUCTO (Simplificado para Angular)
    public async Task<ProductResponseDto?> GetProduct(int id)
    {
        return await db.Products
            .Where(p => p.id == id)
            .Select(p => new ProductResponseDto
            {
                id = p.id ?? 0,
                title = p.title,
                description = p.description,
                price = p.price,
                discountPercentage = p.discountPercentage,
                rating = p.rating,
                stock = p.stock,
                sku = p.sku,
                weight = p.weight,
                width = p.width,
                height = p.height,
                depth = p.depth,
                warrantyInformation = p.warrantyInformation,
                shippingInformation = p.shippingInformation,
                availabilityStatus = p.availabilityStatus,
                returnPolicy = p.returnPolicy,
                minimumOrderQuantity = p.minimumOrderQuantity,
                thumbnail = p.thumbnail,
                categoryId = p.categoryId,
                brandId = p.brandId,
                isActive = p.isActive,
                // Mapeo de Atributos Extra
                extraAttributes = p.attributeValues.Select(av => new ExtraAttributeDto
                {
                    name = av.attributeDefinition.name,
                    value = av.value,
                    dataType = av.attributeDefinition.dataType
                }).ToList(),
                // Mapeo de Imágenes adicionales
                images = p.images.Select(img => new ProductImage
                {
                    id = img.id,
                    imageUrl = img.imageUrl,
                    productId = img.productId
                }).ToList(),

                // Mapeo de Tags
                tags = p.tags.Select(tag => new ProductTag
                {
                    id = tag.id,
                    tagName = tag.tagName,
                    productId = tag.productId
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(List<Product> Items, int TotalCount)> GetProductsAsync(
    int? page,
    int? size,
    string? search,
    int? categoryId,
    int? brandId,
    decimal? minPrice,
    decimal? maxPrice,
    string? sortBy,
    string? order,
    bool? isActive = true)
    {
        var query = db.Products
            .Include(p => p.category)
            .Include(p => p.brand)
            .Include(p => p.tags)
            .Include(p => p.attributeValues)
                .ThenInclude(av => av.attributeDefinition)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.isActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.title.Contains(search) || p.description.Contains(search));

        if (categoryId.HasValue && categoryId > 0)
            query = query.Where(p => p.categoryId == categoryId);

        if (brandId.HasValue && brandId > 0)
            query = query.Where(p => p.brandId == brandId);

        if (minPrice.HasValue)
            query = query.Where(p => p.price >= minPrice);

        if (maxPrice.HasValue)
            query = query.Where(p => p.price <= maxPrice);

        int totalCount = await query.CountAsync();

        // Ordenamiento
        query = sortBy?.ToLower() switch
        {
            "price" => order?.ToLower() == "asc" ? query.OrderBy(p => p.price) : query.OrderByDescending(p => p.price),
            "title" => order?.ToLower() == "desc" ? query.OrderByDescending(p => p.title) : query.OrderBy(p => p.title),
            _ => order?.ToLower() == "asc" ? query.OrderBy(p => p.rating) : query.OrderByDescending(p => p.rating)
        };

        var items = await query
            .Skip(((page ?? 1) - 1) * (size ?? 25))
            .Take(size ?? 25)
            .ToListAsync();

        return (Items: items, TotalCount: totalCount);
    }


    public async Task<(List<ProductAdminDto> Items, int TotalCount)> GetProductsAdminAsync(
    int? page,
    int? size,
    string? search,
    int? categoryId,
    int? brandId,
    decimal? minPrice,
    decimal? maxPrice,
    string? sortBy,
    string? order,
    bool? isActive = null) // "null" Para el Admin, el filtro de isActive es opcional (puede querer ver solo activos, solo inactivos o ambos)
    {
        var query = db.Products.AsQueryable();

        // 0. Filtrado por Estado (Soft Delete)
        // Si isActive tiene valor (true/false), filtramos por él. Si es null, trae todos.
        if (isActive.HasValue)
            query = query.Where(p => p.isActive == isActive.Value);

        // 1. Filtrado por Texto
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.title.Contains(search) || p.description.Contains(search));

        // 2. Filtrado por Categoría
        if (categoryId.HasValue && categoryId > 0)
            query = query.Where(p => p.categoryId == categoryId);

        // 3. Filtrado por Marca
        if (brandId.HasValue && brandId > 0)
            query = query.Where(p => p.brandId == brandId);

        // 4. Filtrado por Rango de Precio
        if (minPrice.HasValue)
            query = query.Where(p => p.price >= minPrice);

        if (maxPrice.HasValue)
            query = query.Where(p => p.price <= maxPrice);

        // 5. Conteo Total (Importante hacerlo antes de la paginación)
        int totalCount = await query.CountAsync();

        // 6. Ordenamiento Dinámico corregido
        if (sortBy?.ToLower() == "price")
        {
            query = order?.ToLower() == "asc"
                ? query.OrderBy(p => p.price)
                : query.OrderByDescending(p => p.price);
        }
        else if (sortBy?.ToLower() == "title")
        {
            query = order?.ToLower() == "asc"
                ? query.OrderBy(p => p.title)
                : query.OrderByDescending(p => p.title);
        }
        // Agregamos explícitamente el caso de ID
        else if (sortBy?.ToLower() == "id")
        {
            query = order?.ToLower() == "asc"
                ? query.OrderBy(p => p.id)
                : query.OrderByDescending(p => p.id);
        }
        else // Por defecto (si sortBy viene nulo o es otra cosa)
        {
            query = query.OrderByDescending(p => p.id);
        }

        // 7. Paginación y ejecución de la consulta
        // IMPORTANTE: Asegúrate de que el Skip/Take vaya al final
        var items = await query
            .Skip(((page ?? 1) - 1) * (size ?? 25))
            .Take(size ?? 25)
            .Select(p => new ProductAdminDto
            {
                id = p.id ?? 0,
                title = p.title,
                sku = p.sku,
                price = p.price,
                stock = p.stock,
                categoryId = p.categoryId,
                brandId = p.brandId,
                isActive = p.isActive,
                imageUrl = p.thumbnail

                // Nota: Asegúrate de incluir aquí categoryId y brandId si los necesitas en el mapeo del Excel
            })
            .ToListAsync();

        return (Items: items, TotalCount: totalCount);
    }

    public async Task CreateProduct(ProductRequestDTo productDto, string scheme, string host)
    {
        var fileService = new FileService(); // Idealmente inyectado por DI

        // Creamos la estrategia de ejecución para permitir transacciones con políticas de reintento
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 1. MAPEO: Transferimos los datos del DTO (que viene del cliente) a la Entidad 'Product'.
                // En este punto, 'product.id' es NULL porque el objeto aún no existe en la base de datos.
                var product = new Product
                {
                    title = productDto.title,
                    description = productDto.description,
                    price = productDto.price,
                    discountPercentage = productDto.discountPercentage,
                    rating = productDto.rating,
                    stock = productDto.stock,
                    sku = productDto.sku,
                    weight = productDto.weight,
                    width = productDto.width,
                    height = productDto.height,
                    depth = productDto.depth,
                    warrantyInformation = productDto.warrantyInformation,
                    shippingInformation = productDto.shippingInformation,
                    availabilityStatus = productDto.availabilityStatus,
                    returnPolicy = productDto.returnPolicy,
                    minimumOrderQuantity = productDto.minimumOrderQuantity,
                    brandId = productDto.brandId,
                    categoryId = productDto.categoryId,
                    isActive = productDto.isActive
                };

                // 2. PROCESAMIENTO: Guardamos la imagen física y actualizamos la propiedad con su URL final.
                product.thumbnail = await fileService.ProcessImage(productDto.thumbnail, scheme, host);

                // 3. GENERACIÓN DE ID: Al ejecutar SaveChanges, SQL Server inserta el registro y genera el ID IDENTITY.
                // Entity Framework recupera ese valor automáticamente y lo asigna a la propiedad 'product.id' en memoria.
                await db.Products.AddAsync(product);
                await db.SaveChangesAsync();

                // 4. RELACIÓN (IMÁGENES): Como 'product.id' ya tiene el valor real devuelto por SQL,
                // lo usamos para establecer la clave foránea (FK) en cada registro de la galería.
                if (productDto.images != null && productDto.images.Any())
                {
                    foreach (var imgDto in productDto.images)
                    {
                        var newImg = new ProductImage
                        {
                            productId = (int)product.id!, // Aquí leemos el IDdel producto que SQL acaba de generar
                            imageUrl = await fileService.ProcessImage(imgDto.imageUrl, scheme, host)
                        };
                        await db.ProductImages.AddAsync(newImg);
                    }
                }

                // 5. RELACIÓN (TAGS): Repetimos el proceso usando el mismo ID del producto padre.
                // SQL Server necesita este valor para saber que estos tags pertenecen a este producto específico.
                if (productDto.tags != null && productDto.tags.Any())
                {
                    foreach (var tagDto in productDto.tags)
                    {
                        var newTag = new ProductTag
                        {
                            productId = (int)product.id!, // El ID ya está disponible tras el SaveChanges del paso 3
                            tagName = tagDto.tagName
                        };
                        await db.ProductTags.AddAsync(newTag);
                    }
                }

                // 6. attributes delproducto ej pulgadas, dimensiones
                if (productDto.extraAttributes != null && productDto.extraAttributes.Any())
                {
                    foreach (var attr in productDto.extraAttributes)
                    {

                        if (int.TryParse(attr.name, out int definitionId))
                        {
                            await db.ProductAttributeValues.AddAsync(new ProductExtraAttributeValue
                            {
                                productId = (int)product.id!,
                                attributeDefinitionId = definitionId,
                                value = attr.value ?? string.Empty
                            });
                        }
                    }
                }

                // Guardamos los hijos (Imágenes y Tags) y confirmamos la transacción de forma atómica.
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Si algo falla, el Rollback asegura que no queden productos creados sin sus fotos o tags.
                await transaction.RollbackAsync();

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string technicalDetail = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                string reportMessage = $"[REPORTE DE ERROR - {timestamp}]\n" +
                                       $"Motivo: Falla en la persistencia del producto.\n" +
                                       $"Detalle Técnico: {technicalDetail}";

                throw new Exception(reportMessage, ex);
            }
        });
    }
    public async Task UpdateProduct(ProductRequestDTo productDto, string scheme, string host)
    {
        var fileService = new FileService();
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 1. Buscamos el producto con sus relaciones cargadas
                var existingProduct = await db.Products
                    .Include(p => p.images)
                    .Include(p => p.tags)
                    .Include(p => p.attributeValues)
                    .FirstOrDefaultAsync(p => p.id == productDto.id);

                if (existingProduct == null)
                    throw new Exception($"Producto con ID {productDto.id} no encontrado.");

                // 2. Actualización de propiedades básicas
                existingProduct.title = productDto.title;
                existingProduct.description = productDto.description;
                existingProduct.price = productDto.price;
                existingProduct.discountPercentage = productDto.discountPercentage;
                existingProduct.rating = productDto.rating;
                existingProduct.stock = productDto.stock;
                existingProduct.sku = productDto.sku;

                // IMPORTANTE: Aseguramos que estos valores se asignen correctamente
                existingProduct.weight = productDto.weight;
                existingProduct.width = productDto.width;
                existingProduct.height = productDto.height;
                existingProduct.depth = productDto.depth;

                // Logística y Garantía
                existingProduct.warrantyInformation = productDto.warrantyInformation ?? string.Empty;
                existingProduct.shippingInformation = productDto.shippingInformation ?? string.Empty;
                existingProduct.availabilityStatus = productDto.availabilityStatus;
                existingProduct.returnPolicy = productDto.returnPolicy ?? string.Empty;
                existingProduct.minimumOrderQuantity = productDto.minimumOrderQuantity;

                existingProduct.categoryId = productDto.categoryId;
                existingProduct.brandId = productDto.brandId;
                existingProduct.isActive = productDto.isActive;

                // 3. Imagen Principal
                if (!string.IsNullOrEmpty(productDto.thumbnail) && productDto.thumbnail.StartsWith("data:image"))
                {
                    existingProduct.thumbnail = await fileService.ProcessImage(productDto.thumbnail, scheme, host);
                }

                // 4. Galería de Imágenes (Limpieza y Re-creación segura)
                // Primero removemos del contexto
                db.ProductImages.RemoveRange(existingProduct.images);
                // Luego limpiamos la lista de la entidad para evitar conflictos de tracking
                existingProduct.images.Clear();

                if (productDto.images != null)
                {
                    foreach (var imgDto in productDto.images)
                    {
                        existingProduct.images.Add(new ProductImage
                        {
                            imageUrl = imgDto.imageUrl.StartsWith("data:image")
                                ? await fileService.ProcessImage(imgDto.imageUrl, scheme, host)
                                : imgDto.imageUrl,
                            productId = existingProduct.id // Aseguramos la relación
                        });
                    }
                }

                // 5. Tags (Limpieza y Re-creación segura)
                db.ProductTags.RemoveRange(existingProduct.tags);
                existingProduct.tags.Clear();

                if (productDto.tags != null)
                {
                    foreach (var tagDto in productDto.tags)
                    {
                        existingProduct.tags.Add(new ProductTag
                        {
                            tagName = tagDto.tagName,
                            productId = existingProduct.id!.Value
                        });
                    }
                }

                // 6. Atributos Extra
                db.ProductAttributeValues.RemoveRange(existingProduct.attributeValues);
                existingProduct.attributeValues.Clear();

                if (productDto.extraAttributes != null && productDto.extraAttributes.Any())
                {
                    // Traemos todas las definiciones de la categoría de una vez para no hacer mil consultas al server
                    var definitions = await db.ProductAttributeDefinitions
                        .Where(d => d.categoryId == existingProduct.categoryId)
                        .ToListAsync();

                    foreach (var attr in productDto.extraAttributes)
                    {
                        int? finalDefId = null;

                        // Intentamos primero si el name es el ID
                        if (int.TryParse(attr.name, out int defId))
                        {
                            finalDefId = defId;
                        }
                        else
                        {
                            // Buscamos por nombre ignorando mayúsculas/minúsculas y espacios
                            var definition = definitions.FirstOrDefault(x =>
                                x.name.Trim().Equals(attr.name.Trim(), StringComparison.OrdinalIgnoreCase));

                            if (definition != null)
                            {
                                finalDefId = definition.id;
                            }
                        }

                        if (finalDefId.HasValue)
                        {
                            existingProduct.attributeValues.Add(new ProductExtraAttributeValue
                            {
                                productId = existingProduct.id!.Value, // Aseguramos el ID del producto
                                attributeDefinitionId = finalDefId.Value,
                                value = attr.value ?? string.Empty // Evitamos nulls en la DB
                            });
                        }
                    }
                }

                // Forzamos que EF detecte los cambios en la entidad principal
                db.Entry(existingProduct).State = EntityState.Modified;

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error al actualizar: {ex.Message}", ex);
            }
        });
    }

    public async Task DeleteProduct(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product != null)
        {
            product.isActive = false; // Solo cambiamos el estado (lo damos de baja lógica sin borrarlo de la base de datos)
            db.Entry(product).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
    }
}