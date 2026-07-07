using ApiComponents.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Infrastructure.Configurations
{
    // Implementa IEntityTypeConfiguration para el modelo Country
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            // Configura la entidad Country para asegurar que la columna Description sea única
            builder.HasIndex(c => c.description)
                .IsUnique();
        }
    }
}