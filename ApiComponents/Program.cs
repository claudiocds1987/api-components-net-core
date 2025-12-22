
using ApiComponents.Persistence.Context;
using ApiComponents.Persistence.Repositories;
using ApiComponents.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN DE CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:5000") // URL de tu Angular cuando este en github poner la url del deploy
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- 2. CONFIGURACIÓN DE JSON (Para evitar conflictos de mayúsculas entre .NET y Angular) ---
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Mantiene los nombres del DTO
    });

// Cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("Connection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// --- 3. INYECCIÓN DE DEPENDENCIAS ---
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IMercadoPagoService, MercadoPagoService>();

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

// --- 4. MIDDLEWARES ---

// ¡IMPORTANTE! El UseCors debe ir DESPUÉS de UseRouting (si estuviera) y ANTES de UseAuthorization
app.UseCors("AllowAngular");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiComponents v1");
        c.RoutePrefix = "swagger";
    });
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

public partial class Program { }


