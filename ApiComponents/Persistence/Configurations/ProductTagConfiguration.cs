using ApiComponents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Persistence.Configurations
{
    public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
    {
        public void Configure(EntityTypeBuilder<ProductTag> builder)
        {
            builder.ToTable("ProductTags");

            builder.HasKey(t => t.id);

            builder.Property(t => t.tagName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(t => t.productId)
                   .IsRequired();

            // Opcional: Crear un índice en tagName para que 
            // buscar productos por "smartwatch" sea ultra rápido.
            builder.HasIndex(t => t.tagName);
        }
    }
}