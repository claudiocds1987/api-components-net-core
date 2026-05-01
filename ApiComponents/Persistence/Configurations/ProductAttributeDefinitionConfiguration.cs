using ApiComponents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Persistence.Configurations
{
    public class ProductAttributeDefinitionConfiguration : IEntityTypeConfiguration<ProductExtraAttributeDefinition>
    {
        public void Configure(EntityTypeBuilder<ProductExtraAttributeDefinition> builder)
        {
            builder.ToTable("ProductAttributeDefinitions");

            builder.HasKey(a => a.id);

            builder.Property(a => a.name)
                .IsRequired()
                .HasMaxLength(100);

            // Índice para que no haya nombres duplicados (Ej: No tener dos "Pulgadas")
            builder.HasIndex(a => a.name).IsUnique();
        }
    }
}