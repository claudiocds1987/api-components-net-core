
using Swashbuckle.AspNetCore.SwaggerGen; // Asegura la referencia a Swashbuckle
using Swashbuckle.AspNetCore.SwaggerUI;  // Asegura la referencia a Swashbuckle
using Swashbuckle.AspNetCore.Swagger;    // Asegura la referencia a Swashbuckle
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ApiComponents.Persistence.Context;
using ApiComponents.Persistence.Repositories;
using ApiComponents.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Creación variables para la cadena de conexión a la base de datos
var connectionString = builder.Configuration.GetConnectionString("Connection"); // Obtiene la cadena de conexión desde appsettings.json
// Registrar servicio para la conexión a la base de datos
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString)
);
// INYECCIÓN DE DEPENDENCIAS DEL REPOSITORIO 
// 1. Repositorio de Empleados, Country, Order (para mercado pago)
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

//  INYECCIÓN DE DEPENDENCIAS DE SERVICIOS
// 1. Servicio de Employee, Country,MercadoPago
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IMercadoPagoService, MercadoPagoService>();
builder.Services.AddControllers();

// Configurar Swagger/OpenAPI (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ApiComponents API",
        Version = "v1"
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiComponents v1");
        // Mantener la UI bajo /swagger (ruta por defecto -> /swagger/index.html)
        c.RoutePrefix = "swagger";
    });

    // app.MapOpenApi(); // Elimina si no usas NSwag
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/swagger-ui", (HttpContext ctx) =>
{
    ctx.Response.Redirect("/swagger/index.html", permanent: false);
    return Results.Empty;
});

app.Run();

// Solo para que las herramientas de Migración puedan inicializar el Host
public partial class Program { }

// Verifica que el paquete NuGet "Swashbuckle.AspNetCore" esté instalado en tu proyecto.
// Si no está instalado, ejecuta el siguiente comando en la consola del Administrador de paquetes NuGet:
// Install-Package Swashbuckle.AspNetCore

