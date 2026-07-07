using ApiComponents.Domain.Models;
using ApiComponents.Infrastructure.Context;


namespace ApiComponents.GraphQL;

public class Query
{
    // El atributo [UsePaging] habilita la paginación automática (estilo Relay)
    // [UseProjection] es la clave: hace que el SQL solo traiga las columnas que el Front pidió
    // [UseFiltering] y [UseSorting] habilitan filtros y ordenamiento dinámico
    [UsePaging(IncludeTotalCount = true, DefaultPageSize = 25)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts(AppDbContext context)
    {
        // Importante: Devolvemos IQueryable para que HotChocolate 
        // pueda "armar" la consulta SQL final según lo que pida el usuario.
        return context.Products;
    }
}