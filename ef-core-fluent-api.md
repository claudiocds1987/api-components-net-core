# Reference: EF Core Fluent API Configurations

## Role & Responsibility
Maneja de manera avanzada y desacoplada las configuraciones del modelo físico de la base de datos. Convierte las clases de C# (Entities) en tablas de bases de datos mediante los `DbSet<T>` configurados con Fluent API.

## Strict Rules
- **Clean DbContext:** El archivo `AppDbContext.cs` debe permanecer limpio. No debe contener bloques masivos de configuración dentro de `OnModelCreating`.
- **Separate Files:** Cada entidad debe tener su propia clase de configuración que implemente `IEntityTypeConfiguration<T>`.
- **Explicit Configurations:** Se deben declarar explícitamente las claves primarias, relaciones, índices únicos, nombres de tablas y restricciones de longitud (`HasMaxLength`).

## Correct Implementation Example

### AppDbContext.cs
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Expone las herramientas para manipular la tabla
    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Aplica automáticamente todas las configuraciones del ensamblado local
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}