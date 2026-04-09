using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Persistence.Context;
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

    //public async Task<(List<Product> Items, int TotalCount)> GetProductsAsync(
    //int? page,
    //int? size,
    //string? search,
    //int? categoryId,
    //int? brandId,
    //decimal? minPrice,
    //decimal? maxPrice,
    //string? sortBy,
    //string? order,
    //bool? isActive = true)
    //{
    //    var query = db.Products.AsQueryable();

    //    // 0. Filtrado por Estado (Soft Delete)
    //    // Si isActive tiene valor (true/false), filtramos por él. Si es null, trae todos.
    //    if (isActive.HasValue)
    //        query = query.Where(p => p.isActive == isActive.Value);

    //    // 1. Filtrado por Texto
    //    if (!string.IsNullOrWhiteSpace(search))
    //        query = query.Where(p => p.title.Contains(search) || p.description.Contains(search));

    //    // 2. Filtrado por Categoría
    //    if (categoryId.HasValue && categoryId > 0)
    //        query = query.Where(p => p.categoryId == categoryId);

    //    // 3. Filtrado por Marca
    //    if (brandId.HasValue && brandId > 0)
    //        query = query.Where(p => p.brandId == brandId);

    //    // 4. Filtrado por Rango de Precio
    //    if (minPrice.HasValue)
    //        query = query.Where(p => p.price >= minPrice);

    //    if (maxPrice.HasValue)
    //        query = query.Where(p => p.price <= maxPrice);

    //    // 5. Conteo Total (Importante hacerlo antes de la paginación)
    //    int totalCount = await query.CountAsync();

    //    // 6. Ordenamiento Dinámico
    //    if (sortBy?.ToLower() == "price")
    //    {
    //        query = order?.ToLower() == "asc"
    //            ? query.OrderBy(p => p.price)
    //            : query.OrderByDescending(p => p.price);
    //    }
    //    else if (sortBy?.ToLower() == "title")
    //    {
    //        query = order?.ToLower() == "desc"
    //            ? query.OrderByDescending(p => p.title)
    //            : query.OrderBy(p => p.title);
    //    }
    //    else // Por defecto si no es precio ni título por default es por "rating" "asc"
    //    {

    //        query = order?.ToLower() == "asc"
    //            ? query.OrderBy(p => p.rating)
    //            : query.OrderByDescending(p => p.rating);
    //    }

    //    // 7. Paginación y ejecución de la consulta
    //    // Usamos .ToListAsync() para asegurar que el retorno sea un List<Product>
    //    var items = await query
    //        .Skip(((page ?? 1) - 1) * (size ?? 25))
    //        .Take(size ?? 25)
    //        .ToListAsync();

    //    return (Items: items, TotalCount: totalCount);
    //}

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
                id = p.id,
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


    public async Task UpdateProduct(Product product)
    {
        db.Entry(product).State = EntityState.Modified;
        await db.SaveChangesAsync();
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