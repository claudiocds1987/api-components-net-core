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

            // RELACIÓN: Producto -> Categoría
            builder.HasOne(p => p.category)
                   .WithMany() // Si ProductCategory no tiene una lista de productos, se deja vacío
                   .HasForeignKey(p => p.categoryId)
                   .OnDelete(DeleteBehavior.Restrict); // No permite borrar categoría si tiene productos

            // RELACIÓN: Producto -> Marca
            builder.HasOne(p => p.brand)
                   .WithMany()
                   .HasForeignKey(p => p.brandId)
                   .OnDelete(DeleteBehavior.Restrict);

            // RELACIÓN: Producto -> Imágenes (Uno a Muchos)
            builder.HasMany(p => p.images)
                   .WithOne()
                   .HasForeignKey(i => i.productId)
                   .OnDelete(DeleteBehavior.Cascade); // Si borras el producto, se borran sus fotos

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