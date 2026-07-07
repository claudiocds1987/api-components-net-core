using ApiComponents.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Infrastructure.Configurations
{
    // Implementa IEntityTypeConfiguration para el modelo Position
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            // Configura la entidad Position para asegurar que la columna Description sea única
            builder.HasIndex(p => p.description)
                .IsUnique();
        }
    }
}