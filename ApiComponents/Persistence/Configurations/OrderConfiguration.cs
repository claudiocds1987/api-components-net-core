using ApiComponents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // Nombre de la tabla
            builder.ToTable("Orders");

            // Clave primaria
            builder.HasKey(o => o.Id);

            // SOLUCIÓN AL WARNING: Especificar precisión para el decimal
            // 18 dígitos en total, 2 de ellos decimales (moneda estándar)
            builder.Property(o => o.TotalAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            // Otras configuraciones útiles
            builder.Property(o => o.PreferenceId)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(o => o.Status)
                   .HasMaxLength(50);
        }
    }
}