using ApiComponents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Persistence.Configurations
{
    public class ProductBrandConfiguration : IEntityTypeConfiguration<ProductBrand>
    {
        public void Configure(EntityTypeBuilder<ProductBrand> builder)
        {
            builder.ToTable("ProductBrands");
            builder.HasKey(b => b.id);

            builder.Property(b => b.name)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.HasIndex(b => b.name).IsUnique();
        }
    }
}