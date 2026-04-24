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

    public async Task<Product?> GetProduct(int id)
    {
        return await db.Products
          .Include(p => p.images)
          .Include(p => p.tags)
          .Include(p => p.reviews)
          .FirstOrDefaultAsync(p => p.id == id);
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
                // 1. Buscamos el producto existente con sus relaciones
                var existingProduct = await db.Products
                    .Include(p => p.images)
                    .Include(p => p.tags)
                    .FirstOrDefaultAsync(p => p.id == productDto.id);

                if (existingProduct == null)
                    throw new Exception($"Producto con ID {productDto.id} no encontrado.");

                // 2. Actualizamos propiedades básicas
                existingProduct.title = productDto.title;
                existingProduct.description = productDto.description;
                existingProduct.price = productDto.price;
                existingProduct.discountPercentage = productDto.discountPercentage;
                existingProduct.stock = productDto.stock;
                existingProduct.sku = productDto.sku;
                existingProduct.categoryId = productDto.categoryId;
                existingProduct.brandId = productDto.brandId;
                existingProduct.isActive = productDto.isActive;
                // ... (agrega el resto de campos como weight, width, etc. igual que en el Create)

                // 3. Actualizamos Imagen Principal (Thumbnail)
                // Solo procesamos si el front manda una nueva base64
                if (productDto.thumbnail.StartsWith("data:image"))
                {
                    existingProduct.thumbnail = await fileService.ProcessImage(productDto.thumbnail, scheme, host);
                }

                // 4. Actualizamos Galería (Borrar y Recrear)
                db.ProductImages.RemoveRange(existingProduct.images);
                if (productDto.images != null && productDto.images.Any())
                {
                    foreach (var imgDto in productDto.images)
                    {
                        existingProduct.images.Add(new ProductImage
                        {
                            imageUrl = imgDto.imageUrl.StartsWith("data:image")
                                ? await fileService.ProcessImage(imgDto.imageUrl, scheme, host)
                                : imgDto.imageUrl // Si ya es URL, la dejamos igual
                        });
                    }
                }

                // 5. Actualizamos Tags (Borrar y Recrear)
                db.ProductTags.RemoveRange(existingProduct.tags);
                if (productDto.tags != null && productDto.tags.Any())
                {
                    foreach (var tagDto in productDto.tags)
                    {
                        existingProduct.tags.Add(new ProductTag { tagName = tagDto.tagName });
                    }
                }

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