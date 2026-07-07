using ApiComponents.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Infrastructure.Configurations
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

            // Configuramos la nueva columna para el JSON
            builder.Property(a => a.validationsJson)
                .HasColumnType("nvarchar(max)"); // Suficiente espacio para cualquier regla futura

            // REFACTORIZACIÓN DE ÍNDICE:
            // Quitamos el índice único de 'name' solo.
            // Ahora: El nombre debe ser único PERO POR CATEGORÍA.
            builder.HasIndex(a => new { a.name, a.categoryId }).IsUnique();
        }
    }
}