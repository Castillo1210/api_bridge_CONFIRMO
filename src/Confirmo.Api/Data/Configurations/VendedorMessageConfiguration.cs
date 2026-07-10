using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class VendedorMessageConfiguration : IEntityTypeConfiguration<VendedorMessage>
{
    public void Configure(EntityTypeBuilder<VendedorMessage> builder)
    {
        builder.ToTable("vendedor_message", "public");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(m => m.VendedorId).IsRequired();
        builder.Property(m => m.SenderType).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Content).IsRequired().HasColumnType("text");
        builder.Property(m => m.MessageType).IsRequired().HasMaxLength(20);
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(m => new { m.VendedorId, m.CreatedAt }).HasDatabaseName("idx_vendedor_messages_vendedor_created");

        builder.HasOne(m => m.Vendedor)
            .WithMany()
            .HasForeignKey(m => m.VendedorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}