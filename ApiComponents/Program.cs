using ApiComponents.Persistence.Context;
using ApiComponents.Persistence.Repositories;
using ApiComponents.Persistence.Seed;
using ApiComponents.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. PARCHE DE SEGURIDAD PARA HOSTING (TLS) ---
// Obligatorio para que MonsterASP pueda hablar con los servidores de Mercado Pago
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

// --- 2. CONFIGURACIÓN DE CORS DINÁMICA ---
var allowedOrigins = builder.Configuration["AllowedOrigins"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // EN LOCAL: Aceptamos todo, pero solo una vez.
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // EN PRODUCCIÓN: Filtramos para GitHub
            if (!string.IsNullOrEmpty(allowedOrigins))
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
        }
    });
});

// --- 3. CONFIGURACIÓN DE JSON ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 1. Evita el error de ciclos infinitos al serializar relaciones
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        // 2. Mantiene los nombres de las propiedades tal cual están en C# (id, title, price)
        // en lugar de convertirlos a camelCase (ID, Title, Price) automáticamente.
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

// --- 5. INYECCIÓN DE DEPENDENCIAS REPOSITORIOS ---
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddHttpClient<IGeminiRepository, GeminiRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
// ---  INYECCIÓN DE DEPENDENCIAS SERVICIOS ---
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IMercadoPagoService, MercadoPagoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBrandService, BrandService>();

// --- 6. CONFIGURAR JWT ---
var jwtKey = builder.Configuration["Jwt:Key"];
// Validación de seguridad para evitar el crash 500.30 si falta la key
if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT Key is missing in configuration!");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// --- 7. SWAGGER ---
// Swagger siempre disponible para pruebas directas
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ApiComponents API",
        Version = "v1",
        Description = "API de Componentes - .NET 8"
    });
});

var app = builder.Build();

// --- 8. MIDDLEWARES (Orden Crítico) ---
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiComponents v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- 9. SEED DATA ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    try
    {
        // Esto ejecutará tanto categorías como marcas etc..
        await DbSeeder.SeedAll(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al cargar los datos iniciales: {ex.Message}");
    }
}

app.Run();

public partial class Program { }
