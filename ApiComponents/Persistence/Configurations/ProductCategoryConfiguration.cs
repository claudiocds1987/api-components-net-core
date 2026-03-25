using ApiComponents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Persistence.Configurations
{
    public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.ToTable("ProductCategories");
            builder.HasKey(c => c.id);

            builder.Property(c => c.name)
                   .HasMaxLength(200)
                   .IsRequired();

            // Nueva configuración para isActive
            builder.Property(c => c.isActive)
                   .HasDefaultValue(true)
                   .IsRequired();

            builder.HasIndex(c => c.isActive)
                   .HasFilter("[isActive] = 1");

            builder.HasIndex(c => c.name).IsUnique();
        }
    }
}