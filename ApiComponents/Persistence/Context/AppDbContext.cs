using ApiComponents.Domain;
using ApiComponents.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiComponents.Persistence.Context
{
    public class AppDbContext : DbContext
    {
        // 1. **CONSTRUCTOR VACÍO AGREGADO** (Necesario para herramientas de diseño/migraciones)
        public AppDbContext()
        {
        }

        // Constructor para inicalizar la base de datos
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Position> Position { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<Order> Orders { get; set; } // para mercado pago 
        public DbSet<User> Users { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; } // para mercado pago 

        // TABLAS DE PRODUCTOS ---
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ProductExtraAttributeDefinition> ProductAttributeDefinitions { get; set; }
        public DbSet<ProductExtraAttributeValue> ProductAttributeValues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // OnModelCreating: Carga todas las configuraciones(ej: descripcion unica, id único) de cada tabla
            // configurado en carpeta Persistence/Configurations
            base.OnModelCreating(modelBuilder);

            // Esta línea le dice a EF Core que aplique todas las clases
            // que implementan IEntityTypeConfiguration<T> en este assembly.
            // Esto importa automáticamente CountryConfiguration, PositionConfiguration,
            // ProductConfiguration, ProductBrandConfiguration, etc.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            //  CONFIGURACIÓN DE ID AUTOINCREMENTAL
            // (Nota: Esto ya podría estar dentro de ProductConfiguration, pero no molesta tenerlo aquí)
            modelBuilder.Entity<Product>().HasKey(p => p.id);

            // 3. CONFIGURACIÓN DE DECIMALES (Solución al error CS1501)
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                // Separamos la precisión de la escala para cumplir con la nueva versión de EF Core
                property.SetPrecision(18); // Dígitos totales
                property.SetScale(2);     // 2 Dígitos después de la coma (ej: 99.99)
            }
        }
    }
}