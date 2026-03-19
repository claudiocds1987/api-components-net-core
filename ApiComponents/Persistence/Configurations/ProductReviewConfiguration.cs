using ApiComponents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Persistence.Configurations
{
    public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.ToTable("ProductReviews");

            builder.HasKey(r => r.id);

            builder.Property(r => r.rating)
                   .IsRequired();

            builder.Property(r => r.comment)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(r => r.userName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(r => r.userEmail)
                   .IsRequired()
                   .HasMaxLength(255);

            // Configuramos la fecha para que SQL Server la asigne 
            // automáticamente si no se envía desde el código.
            builder.Property(r => r.date)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(r => r.productId)
                   .IsRequired();
        }
    }
}