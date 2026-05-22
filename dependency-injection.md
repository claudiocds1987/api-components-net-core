# Reference: Dependency Injection (DI)

## Role & Responsibility
Regula la inyección de dependencias del proyecto en la raíz de composición (`Program.cs`), asegurando que las clases dependan de abstracciones (interfaces) y tengan el ciclo de vida apropiado.

## Strict Rules
- **Interface Registration:** Registrar siempre los servicios y repositorios emparejados con sus respectivas interfaces.
- **Service Lifetime:** Utilizar `Scoped` (`AddScoped`) por defecto para servicios de negocio y repositorios. Esto garantiza el aislamiento de datos por cada solicitud HTTP.

## Correct Implementation Example (`Program.cs`)
```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Base de Datos e Infraestructura
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Registro de la Capa de Persistencia (Repositories)
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

// 3. Registro de la Capa de Aplicación (Services)
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddControllers();