using ApiComponents.Persistence.Context;
using ApiComponents.Persistence.Repositories;
using ApiComponents.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// --- 1. PARCHE DE SEGURIDAD PARA HOSTING (TLS) ---
// Obligatorio para que MonsterASP pueda hablar con los servidores de Mercado Pago
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

// --- 2. CONFIGURACIÓN DE CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "https://claudiocds1987.github.io", // Produccion github pages
            "http://localhost:4200", //Puerto estándar de Angular
            "https://localhost:4200",
            "http://localhost:5000", // Puerto Local e-commerce-v20 Angular 
            "https://localhost:5000") // En caso de usar SSL en local
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// --- 3. CONFIGURACIÓN DE JSON ---
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// --- 4. CONFIGURACIÓN DE BASE DE DATOS ---
var connectionString = builder.Configuration.GetConnectionString("Connection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        );
    }));

// --- 5. INYECCIÓN DE DEPENDENCIAS ---
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IMercadoPagoService, MercadoPagoService>();
builder.Services.AddHttpClient<IGeminiRepository, GeminiRepository>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

// --- 6. SWAGGER ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ApiComponents API",
        Version = "v1",
        Description = "API de Componentes - .NET 8 con Variables de Entorno"
    });
});

var app = builder.Build();

// --- 7. MIDDLEWARES (Orden Crítico) ---

// Swagger siempre disponible para pruebas directas
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiComponents v1");
    c.RoutePrefix = string.Empty;
});

// Habilitar redirección a HTTPS (Fundamental para producción)
app.UseHttpsRedirection();

// UseRouting SIEMPRE antes de UseCors
app.UseRouting();

// Aplicar la política de CORS configurada arriba
app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
