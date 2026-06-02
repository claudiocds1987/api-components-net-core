// ApiComponents/Persistence/Configurations/OrderConfiguration.cs
using ApiComponents.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.id);

            builder.Property(o => o.preferenceId).HasMaxLength(200).IsRequired(false);
            builder.Property(o => o.totalAmount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(o => o.status).HasMaxLength(50).IsRequired();
            builder.Property(o => o.customerEmail).HasMaxLength(256).IsRequired();
            builder.Property(o => o.customerName).HasMaxLength(150).IsRequired();
            builder.Property(o => o.customerPhone).HasMaxLength(50).IsRequired(false);
            builder.Property(o => o.shippingAddress).HasMaxLength(500).IsRequired();
            builder.Property(o => o.shippingCity).HasMaxLength(150).IsRequired();
            builder.Property(o => o.shippingZipCode).HasMaxLength(20).IsRequired();

            // Configurar relación 1:N con OrderDetails
            builder.HasMany(o => o.orderDetails)
                   .WithOne(d => d.order)
                   .HasForeignKey(d => d.orderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}