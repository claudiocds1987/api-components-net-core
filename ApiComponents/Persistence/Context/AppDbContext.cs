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
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
            
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Position> Position { get; set; }
        public DbSet<Country> Country { get; set; }

        public DbSet<Order> Orders { get; set; } // para mercado pago 

        // 2. OnModelCreating: Carga todas las configuraciones (ej: descripcion unica, id único) de cada tabla
        //    configurado en carpeta Persistence/Configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Esta línea le dice a EF Core que aplique todas las clases
            // que implementan IEntityTypeConfiguration<T> en este assembly.
            // Esto importa automáticamente CountryConfiguration y PositionConfiguration.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
