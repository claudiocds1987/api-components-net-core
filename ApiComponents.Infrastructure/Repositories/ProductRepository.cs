using ApiComponents.Application.DTOs;
using ApiComponents.Application.Interfaces;
using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using ApiComponents.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Infrastructure.Repositories;

public class ProductRepository(AppDbContext db, IFileService fileService) : IProductRepository
{
    private readonly IFileService _fileService = fileService;

    public async Task AddProductsList(List<Product> products, CancellationToken cancellationToken = default)
    {
        await db.Products.AddRangeAsync(products, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistProduct(string title, CancellationToken cancellationToken = default)
    => await db.Products.AnyAsync(p => EF.Functions.Like(p.title, title), cancellationToken);

    // 1. OBTENER UN SOLO PRODUCTO (Simplificado para Angular)
    public async Task<ProductResponseDto?> GetProduct(int id, CancellationToken cancellationToken = default)
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
                extraAttributes = p.extraAttributeValues.Select(av => new ExtraAttributeDto
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
            .FirstOrDefaultAsync(cancellationToken);
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
    bool? isActive = true,
    CancellationToken cancellationToken = default)
    {
        var query = db.Products
            .Include(p => p.category)
            .Include(p => p.brand)
            .Include(p => p.tags)
            .Include(p => p.extraAttributeValues)
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

        int totalCount = await query.CountAsync(cancellationToken);

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
            .ToListAsync(cancellationToken);

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
    bool? isActive = null, // "null" Para el Admin, el filtro de isActive es opcional (puede querer ver solo activos, solo inactivos o ambos)
    CancellationToken cancellationToken = default)
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
        int totalCount = await query.CountAsync(cancellationToken);

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

    public async Task CreateProduct(ProductRequestDTo productDto, string scheme, string host, CancellationToken cancellationToken = default)
    {
        // FileService ahora se inyecta por DI

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

                // 2. PROCESAMIENTO: Evaluamos si laimagen es Base64 o una URL directa (ej: Cloudinary)
                if (!string.IsNullOrEmpty(productDto.thumbnail))
                {
                    if (productDto.thumbnail.StartsWith("data:image"))
                    {
                        // Caso A: Es una imagen subida localmente en Base64, se procesa en el servidor
                        product.thumbnail = await _fileService.ProcessImage(productDto.thumbnail, scheme, host, cancellationToken);
                    }
                    else
                    {
                        // Caso B: Es una URL directa externa (Cloudinary), se guarda tal cual viene
                        product.thumbnail = productDto.thumbnail;
                    }
                }

                // 3. GENERACIÓN DE ID: Al ejecutar SaveChanges, SQL Server inserta el registro y genera el ID IDENTITY.
                // Entity Framework recupera ese valor automáticamente y lo asigna a la propiedad 'product.id' en memoria.
                await db.Products.AddAsync(product, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);

                // 4. RELACIÓN (IMÁGENES): Como 'product.id' ya tiene el valor real devuelto por SQL,
                // lo usamos para establecer la clave foránea (FK) en cada registro de la galería.
                if (productDto.images != null && productDto.images.Any())
                {
                    foreach (var imgDto in productDto.images)
                    {
                        var newImg = new ProductImage
                        {
                            productId = (int)product.id!, // Aquí leemos el IDdel producto que SQL acaba de generar
                            imageUrl = await _file_service_ProcessImage_async_wrapper(imgDto.imageUrl, scheme, host, cancellationToken)
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
                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Si algo falla, el Rollback asegura que no queden productos creados sin sus fotos o tags.
                await transaction.RollbackAsync(cancellationToken);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string technicalDetail = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                string reportMessage = $"[REPORTE DE ERROR - {timestamp}]\n" +
                                       $"Motivo: Falla en la persistencia del producto.\n" +
                                       $"Detalle Técnico: {technicalDetail}";

                throw new Exception(reportMessage, ex);
            }
        });
    }

    public async Task<ProductRequestDTo> UpdateProduct(ProductRequestDTo productDto, string scheme, string host, CancellationToken cancellationToken = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        // Creamos la variable local para el retorno final
        ProductRequestDTo resultDto = null!;

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var existingProduct = await db.Products
                    .Include(p => p.images)
                    .Include(p => p.tags)
                    .Include(p => p.extraAttributeValues)
                    .FirstOrDefaultAsync(p => p.id == productDto.id, cancellationToken);

                if (existingProduct == null)
                    throw new Exception($"Producto con ID {productDto.id} no encontrado.");

                // [Mapeo de propiedades primitivas idéntico a tu código]
                existingProduct.title = productDto.title;
                existingProduct.description = productDto.description;
                existingProduct.price = productDto.price;
                existingProduct.discountPercentage = productDto.discountPercentage;
                existingProduct.rating = productDto.rating;
                existingProduct.stock = productDto.stock;
                existingProduct.sku = productDto.sku;
                existingProduct.weight = productDto.weight;
                existingProduct.width = productDto.width;
                existingProduct.height = productDto.height;
                existingProduct.depth = productDto.depth;
                existingProduct.warrantyInformation = productDto.warrantyInformation ?? string.Empty;
                existingProduct.shippingInformation = productDto.shippingInformation ?? string.Empty;
                existingProduct.availabilityStatus = productDto.availabilityStatus;
                existingProduct.returnPolicy = productDto.returnPolicy ?? string.Empty;
                existingProduct.minimumOrderQuantity = productDto.minimumOrderQuantity;
                existingProduct.categoryId = productDto.categoryId;
                existingProduct.brandId = productDto.brandId;
                existingProduct.isActive = productDto.isActive;

                if (!string.IsNullOrEmpty(productDto.thumbnail) && productDto.thumbnail.StartsWith("data:image"))
                {
                    // Caso A: Es una imagen local subida en Base64, se procesa y se guarda localmente
                    existingProduct.thumbnail = await _fileService.ProcessImage(productDto.thumbnail, scheme, host, cancellationToken);
                }
                else
                {
                    // Caso B: Es una URL directa externa (Cloudinary), se guarda tal cual viene
                    existingProduct.thumbnail = productDto.thumbnail;
                }

                // Procesamiento de imágenes secundarias
                db.ProductImages.RemoveRange(existingProduct.images);
                existingProduct.images.Clear();

                if (productDto.images != null)
                {
                    foreach (var imgDto in productDto.images)
                    {
                        existingProduct.images.Add(new ProductImage
                        {
                            imageUrl = imgDto.imageUrl.StartsWith("data:image")
                                ? await _fileService.ProcessImage(imgDto.imageUrl, scheme, host, cancellationToken)
                                : imgDto.imageUrl,
                            productId = existingProduct.id
                        });
                    }
                }

                // Procesamiento de Tags (Corregido el typo de tu código original)
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

                // Procesamiento de Atributos Extra
                db.ProductAttributeValues.RemoveRange(existingProduct.extraAttributeValues);
                existingProduct.extraAttributeValues.Clear();

                if (productDto.extraAttributes != null && productDto.extraAttributes.Any())
                {
                    var definitions = await db.ProductAttributeDefinitions
                        .Where(d => d.categoryId == existingProduct.categoryId)
                        .ToListAsync(cancellationToken);

                    foreach (var attr in productDto.extraAttributes)
                    {
                        int? finalDefId = null;

                        if (int.TryParse(attr.name, out int defId))
                        {
                            finalDefId = defId;
                        }
                        else
                        {
                            var definition = definitions.FirstOrDefault(x =>
                                x.name.Trim().Equals(attr.name.Trim(), StringComparison.OrdinalIgnoreCase));

                            if (definition != null)
                            {
                                finalDefId = definition.id;
                            }
                        }

                        if (finalDefId.HasValue)
                        {
                            existingProduct.extraAttributeValues.Add(new ProductExtraAttributeValue
                            {
                                productId = existingProduct.id!.Value,
                                attributeDefinitionId = finalDefId.Value,
                                value = attr.value ?? string.Empty
                            });
                        }
                    }
                }

                db.Entry(existingProduct).State = EntityState.Modified;

                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // =========================================================================
                // MAPEO DE RETORNO VERIFICADO CON MIS MODELOS REALES
                // =========================================================================
                resultDto = new ProductRequestDTo
                {
                    id = existingProduct.id,
                    title = existingProduct.title,
                    description = existingProduct.description,
                    price = existingProduct.price,
                    discountPercentage = existingProduct.discountPercentage,
                    rating = existingProduct.rating,
                    stock = existingProduct.stock,
                    sku = existingProduct.sku,
                    weight = existingProduct.weight,
                    width = existingProduct.width,
                    height = existingProduct.height,
                    depth = existingProduct.depth,
                    warrantyInformation = existingProduct.warrantyInformation,
                    shippingInformation = existingProduct.shippingInformation,
                    availabilityStatus = existingProduct.availabilityStatus,
                    returnPolicy = existingProduct.returnPolicy,
                    minimumOrderQuantity = existingProduct.minimumOrderQuantity,
                    categoryId = existingProduct.categoryId,
                    brandId = existingProduct.brandId,
                    isActive = existingProduct.isActive,
                    thumbnail = existingProduct.thumbnail,

                    // Mapeando a la lista real de objetos ProductImage
                    images = existingProduct.images.Select(img => new ProductImage
                    {
                        id = img.id,
                        imageUrl = img.imageUrl,
                        productId = img.productId
                    }).ToList(),

                    // Mapeabdo a la lista real de objetos ProductTag
                    tags = existingProduct.tags.Select(t => new ProductTag
                    {
                        id = t.id,
                        tagName = t.tagName,
                        productId = t.productId
                    }).ToList(),

                    // Los extra attributes se devuelven tal como vinieron mapeados en la petición
                    extraAttributes = productDto.extraAttributes ?? []
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new Exception($"Error al actualizar: {ex.Message}", ex);
            }
        });

        return resultDto;
    }

    public async Task<ProductRequestDTo> UpdateProductStatus(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        ProductRequestDTo resultDto = null!;

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Buscamos SOLO el producto raíz (sin relaciones, ahorrando memoria y CPU)
                var existingProduct = await db.Products
                    .FirstOrDefaultAsync(p => p.id == id, cancellationToken);

                if (existingProduct == null)
                    throw new Exception($"Producto con ID {id} no encontrado.");

                // Modificamos únicamente la propiedad deseada
                existingProduct.isActive = isActive;

                db.Entry(existingProduct).State = EntityState.Modified;

                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Devolvemos el DTO con el estado mínimo requerido o el objeto completo mapeado
                resultDto = new ProductRequestDTo
                {
                    id = existingProduct.id,
                    title = existingProduct.title,
                    isActive = existingProduct.isActive
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new Exception($"Error al cambiar estado: {ex.Message}", ex);
            }
        });

        return resultDto;
    }

    // Helper wrapper to call file service without changing many small call sites
    private async Task<string> _file_service_ProcessImage_async_wrapper(string imageData, string scheme, string host, CancellationToken cancellationToken)
    {
        return await _fileService.ProcessImage(imageData, scheme, host, cancellationToken);
    }
}