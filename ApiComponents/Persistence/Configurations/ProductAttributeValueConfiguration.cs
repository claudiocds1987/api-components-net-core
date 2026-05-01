using ApiComponents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Persistence.Configurations
{
    public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductExtraAttributeValue>
    {
        public void Configure(EntityTypeBuilder<ProductExtraAttributeValue> builder)
        {
            builder.ToTable("ProductAttributeValues");

            builder.HasKey(v => v.id);

            builder.Property(v => v.value)
                .IsRequired()
                .HasMaxLength(500); // 500 es suficiente para la mayoría de specs

            // --- RELACIONES ---

            builder.HasOne(v => v.product)
                .WithMany(p => p.attributeValues)
                .HasForeignKey(v => v.productId)
                .OnDelete(DeleteBehavior.Cascade); // Si borras el producto, se borran sus specs

            builder.HasOne(v => v.attributeDefinition)
                .WithMany(d => d.attributeValues)
                .HasForeignKey(v => v.attributeDefinitionId)
                .OnDelete(DeleteBehavior.Restrict); // No borramos la definición si hay valores usándola

            // --- ÍNDICES Y PERFORMANCE ---

            // Optimización para búsquedas rápidas por valor (Filtros en el Frontend)
            builder.HasIndex(v => v.value);

            // Índice compuesto para optimizar búsquedas de Atributo + Valor
            builder.HasIndex(v => new { v.attributeDefinitionId, v.value });

            // REGLA DE NEGOCIO: Evita que un mismo producto tenga dos veces el mismo atributo 
            // (Ej: que no tenga dos resoluciones distintas cargadas por error)
            builder.HasIndex(v => new { v.productId, v.attributeDefinitionId })
                .IsUnique();
        }
    }
}