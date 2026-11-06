using ApiComponents.Application.DTOs;
using ApiComponents.Application.Interfaces;
using ApiComponents.Application.Repositories;
using ApiComponents.Domain.Models;
using ApiComponents.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
                // Estad�sticas precalculadas � se leen directamente desde Products, sin AVG/COUNT din�mico.
                rating = p.rating,
                reviewCount = p.reviewCount,
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
                // Mapeo de Im�genes adicionales
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
                }).ToList(),
                // Seguridad: se proyecta a ProductReviewDto (sin UserEmail).
                reviews = p.reviews
                    .OrderByDescending(r => r.date)
                    .Select(r => new ProductReviewDto
                    {
                        id       = r.id,
                        rating   = r.rating,
                        comment  = r.comment,
                        userName = r.userName,
                        date     = r.date
                    }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetProductsAsync(
    int? page,
    int? size,
    string? search,
    List<int>? categoryIds,
    List<int>? brandIds,
    decimal? minPrice,
    decimal? maxPrice,
    string? sortBy,
    string? order,
    bool? isActive = true,
    decimal? minRating = null,
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

        if (categoryIds != null && categoryIds.Count > 0)
        {
            var validCatIds = categoryIds.Where(id => id > 0).ToList();
            if (validCatIds.Count > 0)
                query = query.Where(p => validCatIds.Contains(p.categoryId));
        }

        if (brandIds != null && brandIds.Count > 0)
        {
            var validBrandIds = brandIds.Where(id => id > 0).ToList();
            if (validBrandIds.Count > 0)
                query = query.Where(p => validBrandIds.Contains(p.brandId));
        }

        if (minPrice.HasValue)
            query = query.Where(p => p.price >= minPrice);

        if (maxPrice.HasValue)
            query = query.Where(p => p.price <= maxPrice);

        if (minRating.HasValue)
            query = query.Where(p => p.rating >= minRating.Value);

        int totalCount = await query.CountAsync(cancellationToken);

        // Ordenamiento
        query = sortBy?.ToLower() switch
        {
            "price" => order?.ToLower() == "asc" ? query.OrderBy(p => p.price) : query.OrderByDescending(p => p.price),
            "title" => order?.ToLower() == "desc" ? query.OrderByDescending(p => p.title) : query.OrderBy(p => p.title),
            "rating" => order?.ToLower() == "asc" ? query.OrderBy(p => p.rating).ThenByDescending(p => p.id) : query.OrderByDescending(p => p.rating).ThenByDescending(p => p.id),
            _ => order?.ToLower() == "asc" ? query.OrderBy(p => p.rating).ThenByDescending(p => p.id) : query.OrderByDescending(p => p.rating).ThenByDescending(p => p.id)
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
    List<int>? categoryIds,
    List<int>? brandIds,
    decimal? minPrice,
    decimal? maxPrice,
    string? sortBy,
    string? order,
    bool? isActive = null, // "null" Para el Admin, el filtro de isActive es opcional (puede querer ver solo activos, solo inactivos o ambos)
    CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsQueryable();

        // 0. Filtrado por Estado (Soft Delete)
        // Si isActive tiene valor (true/false), filtramos por �l. Si es null, trae todos.
        if (isActive.HasValue)
            query = query.Where(p => p.isActive == isActive.Value);

        // 1. Filtrado por Texto
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.title.Contains(search) || p.description.Contains(search));

        // 2. Filtro combinado de Categor�as
        if (categoryIds != null && categoryIds.Count > 0)
        {
            var validCatIds = categoryIds.Where(id => id > 0).ToList();
            if (validCatIds.Count > 0)
                query = query.Where(p => validCatIds.Contains(p.categoryId));
        }

        // 3. Filtro combinado de Marcas
        if (brandIds != null && brandIds.Count > 0)
        {
            var validBrandIds = brandIds.Where(id => id > 0).ToList();
            if (validBrandIds.Count > 0)
                query = query.Where(p => validBrandIds.Contains(p.brandId));
        }

        // 4. Filtrado por Rango de Precio
        if (minPrice.HasValue)
            query = query.Where(p => p.price >= minPrice);

        if (maxPrice.HasValue)
            query = query.Where(p => p.price <= maxPrice);

        // 5. Conteo Total (Importante hacerlo antes de la paginaci�n)
        int totalCount = await query.CountAsync(cancellationToken);

        // 6. Ordenamiento Din�mico
        var validSortFields = new[] { "price", "title", "id", "discountPercentage", "sku", "stock", "isActive" };

        if (!string.IsNullOrWhiteSpace(sortBy) && validSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
        {
            // Buscamos el nombre correcto de la propiedad (por si el usuario manda "discountpercentage" y la clase es "discountPercentage")
            var propName = typeof(Product).GetProperties()
                .FirstOrDefault(p => p.Name.Equals(sortBy, StringComparison.OrdinalIgnoreCase))?.Name;

            if (propName != null)
            {
                query = ApplyOrdering(query, propName, order ?? "desc");
            }
        }
        else
        {
            query = query.OrderByDescending(p => p.id);
        }

        // 7. Paginaci�n y ejecuci�n de la consulta
        var items = await query
            .Skip(((page ?? 1) - 1) * (size ?? 25))
            .Take(size ?? 25)
            .Select(p => new ProductAdminDto
            {
                id = p.id ?? 0,
                title = p.title,
                sku = p.sku,
                price = p.price,
                discountPercentage = p.discountPercentage,
                stock = p.stock,
                categoryId = p.categoryId,
                brandId = p.brandId,
                isActive = p.isActive,
                imageUrl = p.thumbnail,
                rating = p.rating,
                reviewCount = p.reviewCount

                // Nota: Aseg�rate de incluir aqu� categoryId y brandId si los necesitas en el mapeo del Excel
            })
            .ToListAsync();

        return (Items: items, TotalCount: totalCount);
    }

    public async Task CreateProduct(ProductRequestDTo productDto, string scheme, string host, CancellationToken cancellationToken = default)
    {
        // FileService ahora se inyecta por DI

        // Creamos la estrategia de ejecuci�n para permitir transacciones con pol�ticas de reintento
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 1. MAPEO: Transferimos los datos del DTO (que viene del cliente) a la Entidad 'Product'.
                // En este punto, 'product.id' es NULL porque el objeto a�n no existe en la base de datos.
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

                // 3. GENERACI�N DE ID: Al ejecutar SaveChanges, SQL Server inserta el registro y genera el ID IDENTITY.
                // Entity Framework recupera ese valor autom�ticamente y lo asigna a la propiedad 'product.id' en memoria.
                await db.Products.AddAsync(product, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);

                // 4. RELACI�N (IM�GENES): Como 'product.id' ya tiene el valor real devuelto por SQL,
                // lo usamos para establecer la clave for�nea (FK) en cada registro de la galer�a.
                if (productDto.images != null && productDto.images.Any())
                {
                    foreach (var imgDto in productDto.images)
                    {
                        var newImg = new ProductImage
                        {
                            productId = (int)product.id!, // Aqu� leemos el IDdel producto que SQL acaba de generar
                            imageUrl = await _file_service_ProcessImage_async_wrapper(imgDto.imageUrl, scheme, host, cancellationToken)
                        };
                        await db.ProductImages.AddAsync(newImg);
                    }
                }

                // 5. RELACI�N (TAGS): Repetimos el proceso usando el mismo ID del producto padre.
                // SQL Server necesita este valor para saber que estos tags pertenecen a este producto espec�fico.
                if (productDto.tags != null && productDto.tags.Any())
                {
                    foreach (var tagDto in productDto.tags)
                    {
                        var newTag = new ProductTag
                        {
                            productId = (int)product.id!, // El ID ya est� disponible tras el SaveChanges del paso 3
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

                // Guardamos los hijos (Im�genes y Tags) y confirmamos la transacci�n de forma at�mica.
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
                                       $"Detalle T�cnico: {technicalDetail}";

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

                // [Mapeo de propiedades primitivas id�ntico a tu c�digo]
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

                // Procesamiento de im�genes secundarias
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

                // Procesamiento de Tags (Corregido el typo de tu c�digo original)
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

                    // Los extra attributes se devuelven tal como vinieron mapeados en la petici�n
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
                // Buscamos SOLO el producto ra�z (sin relaciones, ahorrando memoria y CPU)
                var existingProduct = await db.Products
                    .FirstOrDefaultAsync(p => p.id == id, cancellationToken);

                if (existingProduct == null)
                    throw new Exception($"Producto con ID {id} no encontrado.");

                // Modificamos �nicamente la propiedad deseada
                existingProduct.isActive = isActive;

                db.Entry(existingProduct).State = EntityState.Modified;

                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Devolvemos el DTO con el estado m�nimo requerido o el objeto completo mapeado
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

    // M�todo para actualizar el stock de un producto, (Necesario para actualizar stock del producto/os cuando finaliza la orden de compra).
    public async Task UpdateProductStock(int id, int quantityToReduce, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.FindAsync(new object[] { id }, cancellationToken);

        if (product == null)
            throw new Exception($"Producto con ID {id} no encontrado.");

        if (product.stock < quantityToReduce)
            throw new Exception($"Stock insuficiente para el producto '{product.title}'. Stock actual: {product.stock}, Solicitado: {quantityToReduce}.");

        product.stock -= quantityToReduce;

        // Opcional: Si el stock llega a 0, podr�as pasarlo a inactivo o "Out of Stock"
        if (product.stock == 0)
        {
            product.availabilityStatus = "Out of Stock";
        }

        db.Products.Update(product);

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreProductStock(int id, int quantityToRestore, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.FindAsync(new object[] { id }, cancellationToken);

        if (product != null)
        {
            product.stock += quantityToRestore;

            // Si el estado estaba agotado, lo volvemos a poner disponible autom�ticamente
            if (product.availabilityStatus == "Out of Stock" && product.stock > 0)
            {
                product.availabilityStatus = "In Stock";
            }

            db.Products.Update(product);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    // Aplica ordenamiento din�mico a la consulta de productos,
    // seg�n el nombre de la propiedad (sortBy) y la direcci�n (asc/desc).
    private IOrderedQueryable<Product> ApplyOrdering(IQueryable<Product> query, string sortBy, string order)
    {
        var parameter = Expression.Parameter(typeof(Product), "p");
        var property = Expression.Property(parameter, sortBy); // Esto busca la propiedad por su nombre exacto
        var lambda = Expression.Lambda(property, parameter);

        var methodName = order?.ToLower() == "asc" ? "OrderBy" : "OrderByDescending";
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(Product), property.Type);

        return (IOrderedQueryable<Product>)method.Invoke(null, new object[] { query, lambda })!;
    }

    // Helper wrapper to call file service without changing many small call sites
    private async Task<string> _file_service_ProcessImage_async_wrapper(string imageData, string scheme, string host, CancellationToken cancellationToken)
    {
        return await _fileService.ProcessImage(imageData, scheme, host, cancellationToken);
    }

    // ==============================================================================================================
    // M�TODO PARA OBTENER LA CANTIDAD DE PRODUCTOS POR CATEGOR�A (USADO EN COMPONENTE product-filter DE ANGULAR)
    // ==============================================================================================================
    public async Task<IEnumerable<TotalProductsByCategoryDto>> GetTotalProductsByCategoryAsync(bool? isActive = true, CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.isActive == isActive.Value);

        return await query
            .GroupBy(p => p.categoryId)
            .Select(g => new TotalProductsByCategoryDto
            {
                CategoryId = g.Key,
                TotalProducts = g.Count()
            })
            .OrderBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);
    }

    // ==============================================================================================================
    // M�TODO PARA OBTENER LA CANTIDAD DE PRODUCTOS POR MARCA (USADO EN COMPONENTE product-filter DE ANGULAR)
    // ==============================================================================================================
    public async Task<IEnumerable<TotalProductsByBrandDto>> GetTotalProductsByBrandAsync(bool? isActive = true, CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.isActive == isActive.Value);

        return await query
            .GroupBy(p => p.brandId)
            .Select(g => new TotalProductsByBrandDto
            {
                BrandId = g.Key,
                TotalProducts = g.Count()
            })
            .OrderBy(x => x.BrandId)
            .ToListAsync(cancellationToken);
    }

    // ==============================================================================================================
    // M�TODO PARA OBTENER LA CANTIDAD DE PRODUCTOS POR RATING 1-5(USADO EN COMPONENTE product-filter DE ANGULAR)
    // ==============================================================================================================
    public async Task<List<TotalProductsByRatingDto>> GetTotalProductsByRatingAsync(
        bool? isActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.isActive == isActive.Value);

        var ratings = await query.Select(p => p.rating).ToListAsync(cancellationToken);

        return Enumerable.Range(1, 5)
            .Reverse()
            .Select(stars => new TotalProductsByRatingDto
            {
                Rating = stars,
                TotalProducts = ratings.Count(r => r >= stars)
            })
            .ToList();
    }
}
