using ApiComponents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.id);

            // Propiedades básicas
            builder.Property(p => p.title).HasMaxLength(200).IsRequired();
            builder.Property(p => p.sku).HasMaxLength(100).IsRequired();

            // --- NUEVA CONFIGURACIÓN ISACTIVE ---
            builder.Property(p => p.isActive)
                   .HasDefaultValue(true) // Fuerza el 1 (true) a nivel de tabla SQL
                   .IsRequired();

            // Índice para optimizar búsquedas de productos activos
            builder.HasIndex(p => p.isActive)
                   .HasFilter("[isActive] = 1");
            // ------------------------------------

            // RELACIÓN: Producto -> Categoría
            builder.HasOne(p => p.category)
                   .WithMany()
                   .HasForeignKey(p => p.categoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // RELACIÓN: Producto -> Marca
            builder.HasOne(p => p.brand)
                   .WithMany()
                   .HasForeignKey(p => p.brandId)
                   .OnDelete(DeleteBehavior.Restrict);

            // RELACIÓN: Producto -> Imágenes (Uno a Muchos)
            builder.HasMany(p => p.images)
                   .WithOne()
                   .HasForeignKey(i => i.productId)
                   .OnDelete(DeleteBehavior.Cascade);

            // RELACIÓN: Producto -> Tags (Uno a Muchos)
            builder.HasMany(p => p.tags)
                   .WithOne()
                   .HasForeignKey(t => t.productId)
                   .OnDelete(DeleteBehavior.Cascade);

            // RELACIÓN: Producto -> Reviews (Uno a Muchos)
            builder.HasMany(p => p.reviews)
                   .WithOne()
                   .HasForeignKey(r => r.productId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}