
using ApiComponents.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiComponents.Infrastructure.Configurations
{
    public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.ToTable("OrderDetails");
            builder.HasKey(d => d.id);

            builder.Property(d => d.quantity).IsRequired();
            builder.Property(d => d.price).HasColumnType("decimal(18,2)").IsRequired();
        }
    }
}