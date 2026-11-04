using ApiComponents.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Infrastructure.Configurations
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

            // Estadísticas precalculadas de reviews.
            // Se sobreescribe la configuración global (18,2) para usar precisión adecuada a un
            // promedio de 1.00 a 5.00 con 2 decimales (ej: 4.25, 4.75, 3.99).
            builder.Property(p => p.rating)
                   .HasColumnType("decimal(4,2)")
                   .HasDefaultValue(0m)
                   .IsRequired();

            builder.Property(p => p.reviewCount)
                   .HasDefaultValue(0)
                   .IsRequired();

            // --- CONFIGURACIÓN ISACTIVE ---
            builder.Property(p => p.isActive)
                   .HasDefaultValue(true)
                   .IsRequired();

            // Índice para optimizar búsquedas de productos activos
            builder.HasIndex(p => p.isActive)
                   .HasFilter("[isActive] = 1");
            // --------------------------------

            // RELACIÓN: Producto → Categoría
            builder.HasOne(p => p.category)
                   .WithMany()
                   .HasForeignKey(p => p.categoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // RELACIÓN: Producto → Marca
            builder.HasOne(p => p.brand)
                   .WithMany()
                   .HasForeignKey(p => p.brandId)
                   .OnDelete(DeleteBehavior.Restrict);

            // RELACIÓN: Producto → Imágenes (Uno a Muchos)
            builder.HasMany(p => p.images)
                   .WithOne()
                   .HasForeignKey(i => i.productId)
                   .OnDelete(DeleteBehavior.Cascade);

            // RELACIÓN: Producto → Tags (Uno a Muchos)
            builder.HasMany(p => p.tags)
                   .WithOne()
                   .HasForeignKey(t => t.productId)
                   .OnDelete(DeleteBehavior.Cascade);

            // RELACIÓN: Producto → Reviews (Uno a Muchos)
            builder.HasMany(p => p.reviews)
                   .WithOne()
                   .HasForeignKey(r => r.productId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
