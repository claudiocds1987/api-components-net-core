using ApiComponents.Domain.Models;
using ApiComponents.Infrastructure.Context;
namespace ApiComponents.GraphQL;

public class Query
{
    // ✅ Usamos offset/limit en lugar de cursores
    [UseOffsetPaging(IncludeTotalCount = true, DefaultPageSize = 25)]
    [UseProjection] // SQL solo trae las columnas pedidas
    [UseFiltering]  // habilita filtros dinámicos
    [UseSorting]    // habilita ordenamiento dinámico
    public IQueryable<Product> GetProducts(AppDbContext context)
    {
        // Importante: Devolvemos IQueryable para que HotChocolate 
        // pueda "armar" la consulta SQL final según lo que pida el usuario.
        return context.Products;
    }
}