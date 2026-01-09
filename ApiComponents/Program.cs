using ApiComponents.Persistence.Context;
using ApiComponents.Persistence.Repositories;
using ApiComponents.Services;
using Microsoft.AspNetCore.Builder;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.OpenApi;

using System;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN DE CORS ACTUALIZADA ---
builder.Services.AddCors(options =>
{
options.AddPolicy("AllowAngular", policy =>
{
policy.WithOrigins(
        "https://claudiocds1987.github.io",
        "http://localhost:4200", // Agregado para desarrollo local
        "http://localhost:5000" // Agregado para desarrollo local
      )
      .AllowAnyHeader()
      .AllowAnyMethod();
});
});

// --- 2. CONFIGURACIÓN DE JSON ---
builder.Services.AddControllers()
    .AddJsonOptions(options => {
options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// --- 3. CONFIGURACIÓN DE BASE DE DATOS ---
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

// --- 4. INYECCIÓN DE DEPENDENCIAS ---
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IMercadoPagoService, MercadoPagoService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
options.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiComponents API", Version = "v1" });
});

var app = builder.Build();

// --- 5. MIDDLEWARES (Orden Corregido) ---

// Swagger primero para pruebas
app.UseSwagger();
app.UseSwaggerUI(c =>
{
c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiComponents v1");
c.RoutePrefix = string.Empty;
});

// Redirección HTTPS antes de CORS
app.UseHttpsRedirection();

// CORS debe ir después de HttpsRedirection y antes de MapControllers
app.UseCors("AllowAngular");

app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }