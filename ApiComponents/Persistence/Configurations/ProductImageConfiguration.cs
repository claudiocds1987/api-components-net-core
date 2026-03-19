using ApiComponents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Persistence.Configurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");

            builder.HasKey(i => i.id);

            builder.Property(i => i.imageUrl)
                   .IsRequired()
                   .HasMaxLength(500); // Un límite razonable para URLs largas

            // Aunque la relación se define en ProductConfiguration, 
            // aquí reforzamos que el ID del producto es obligatorio.
            builder.Property(i => i.productId)
                   .IsRequired();
        }
    }
}