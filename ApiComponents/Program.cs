using ApiComponents.Infrastructure;
using ApiComponents.Infrastructure.Seed;
using ApiComponents.Services;
using ApiComponents.Infrastructure.Context;
using ApiComponents.GraphQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text;
using ApiComponents.Application.Interfaces;
using ApiComponents.Infrastructure.Services;
using ApiComponents.Application.Mappings;

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
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            if (!string.IsNullOrEmpty(allowedOrigins))
            {
                // Separamos por coma por si algún día se agregan más URLs
                var origins = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);
                policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
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

        // 2. CAMBIO CLAVE: Forzamos CamelCase (minúsculas) para que coincida con Angular
        // Esto convertirá Items -> items, TotalItems -> totalItems automáticamente.
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// --- 4. CONFIGURACIÓN DE BASE DE DATOS Y REPOSITORIOS ---
// Al pasarle 'builder.Configuration', el método tiene acceso al connection string
builder.Services.AddInfrastructure(builder.Configuration);

// --- 4. CONFIGURACIÓN DE BASE DE DATOS ---
//var connectionString = builder.Configuration.GetConnectionString("Connection");
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(connectionString, sqlOptions =>
//    {
//        sqlOptions.EnableRetryOnFailure(
//            maxRetryCount: 5,
//            maxRetryDelay: TimeSpan.FromSeconds(10),
//            errorNumbersToAdd: null
//        );
//    }));

// --- 5. INYECCIÓN DE DEPENDENCIAS REPOSITORIOS ---
// Registrar repositorios (algunos siguen en Persistence; ProductRepository está en Infrastructure)
//builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
//builder.Services.AddScoped<ICountryRepository, CountryRepository>();
//builder.Services.AddScoped<IOrderRepository, OrderRepository>();
//builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddScoped<IGeminiRepository, GeminiRepository>();
// Correct registration: interface and implementation both from Persistence.Repositories
//builder.Services.AddScoped<ApplicationRepos.IProductRepository, ApplicationRepos.ProductRepository>();
//builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
//builder.Services.AddScoped<IBrandRepository, BrandRepository>();

//builder.Services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
//builder.Services.AddScoped<IProductAttributeRepository, ProductAttributeRepository>();
// ---  INYECCIÓN DE DEPENDENCIAS SERVICIOS ---
// IR BOORANDO LOS SERVICIOS DESDE EMPLOYEESERVICE HASTA PRODUCTEXTRAATTRIBUTESERVICE A MEDIDA QUE SE VAN HACIENDO LSO COMMADN Y QUERIES MEDIATOR MENO FILESERVICE Y AUTOMMAPER
//builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IMercadoPagoService, MercadoPagoService>();
//builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGeminiService, GeminiService>();
//builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
builder.Services.AddScoped<IProductExtraAttributeService, ProductExtraAttributeService>();
// Servicio para procesar imágenes/archivos (necesario para repositorios que lo inyectan)
builder.Services.AddScoped<IFileService, FileService>();
// Registro actualizado para AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    // Localiza el ensamblado de MappingProfile y registra todos los perfiles allí
    cfg.AddMaps(typeof(MappingProductExtraAttributesProfile).Assembly);
});

// --- Mediator ---
// Registrar MediatR para handlers ubicados en Application
// Registrar handlers MediatR desde el ensamblado correcto (Features en este proyecto)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApiComponents.Application.Features.Products.Commands.CreateProduct.CreateProductCommandHandler).Assembly));

// --- CONFIGURACIÓN DE GRAPHQL (HotChocolate) ---
// Esto habilita el motor de consultas dinámicas sin afectar a los controladores REST.
// HotChocolate v15 inyectará el DbContext automáticamente porque ya está registrado en builder.Services.
builder.Services
    .AddHttpClient()
    .AddGraphQLServer()
    .AddQueryType<Query>()           // Registra la clase donde definimos las consultas
    .AddProjections()                // Permite que el SQL solo traiga las columnas pedidas desde el Front
    .AddFiltering()                  // Habilita filtros avanzados (where)
    .AddSorting()                   // Habilita ordenamiento dinámico (orderby)
    .ModifyCostOptions(o => o.MaxFieldCost = 10000);

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

    // CONFIGURACIÓN PARA JWT EN SWAGGER
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        // Al usar SecuritySchemeType.Http y Scheme = "bearer", 
        // Swagger añade automáticamente la palabra "Bearer "
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue directamente su token JWT aquí."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// --- Manejo Global de Excepciones ---
app.UseMiddleware<ApiComponents.Middlewares.GlobalExceptionMiddleware>();

// --- 8. MIDDLEWARES (Orden Crítico) ---
// UseCors: Permite (o deniega) que aplicaciones desde otros dominios (como mi app de Angular) consuman la API.
app.UseCors("AllowAngular");
// app.UseStaticFiles(): Permite leer la carpeta en donde estan las imagenes wwwroot/uploads
app.UseStaticFiles();
// UseSwagger(); genera el archivo JSON con la definición. 
app.UseSwagger();
// UseSwaggerUI monta la interfaz gráfica para probar los endpoints.
app.UseSwaggerUI(c =>
{
    // Usamos "./swagger/v1/swagger.json" para forzar que busque en la carpeta actual
    c.SwaggerEndpoint("./swagger/v1/swagger.json", "ApiComponents v1");
    c.RoutePrefix = string.Empty;
});
// UseHttpsRedirection: Fuerza a que cualquier petición HTTP se pase a HTTPS
app.UseHttpsRedirection();
// UseRouting: Este es el "GPS". Analiza la URL y decide a qué controlador pertenece la petición. Antes de CORS y de la Autorización para que el sistema sepa qué reglas aplicar a esa ruta específica.
app.UseRouting();
// UseAuthentication: "¿Quién eres?". Verifica si traes un token válido o una cookie.
app.UseAuthentication();
// UseAuthorization: "¿Tienes permiso?".Una vez que sabemos quién eres, este middleware revisa si tu rol te permite entrar a ese endpoint.
app.UseAuthorization();
// MapControllers: Es el final del camino. Ejecuta la lógica de tu controlador.
app.MapControllers();
// MapGraphQL: Habilita el endpoint único para GraphQL (/graphql)
app.MapGraphQL("/graphql");

// --- 9. MIGRACIONES Y SEED DATA ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    try
    {
        // --- APLICAR MIGRACIONES PENDIENTES ---
        // Esto iguala la estructura de MonsterASP con mi base local automáticamente
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
            Console.WriteLine("Migraciones aplicadas con éxito.");
        }

        // --- CARGAR DATOS INICIALES ---
        // Ejecuta el Seeding de datos maestros (Categorías, Marcas, etc.).
        // El método es idempotente: solo insertará registros si las tablas están vacías (Esto esta configurado en DbSeeders),
        // evitando duplicados en cada reinicio de la aplicación.
        await DbSeeder.SeedAll(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en el proceso de inicio (Migración/Seed): {ex.Message}");
    }
}

app.Run();

public partial class Program { }
