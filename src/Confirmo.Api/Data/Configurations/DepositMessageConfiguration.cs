using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class DepositMessageConfiguration : IEntityTypeConfiguration<DepositMessage>
{
    public void Configure(EntityTypeBuilder<DepositMessage> builder)
    {
        builder.ToTable("deposit_messages", "public");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(m => m.DepositId).IsRequired();
        builder.Property(m => m.SenderType).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Content).IsRequired().HasColumnType("text");
        builder.Property(m => m.MessageType).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Metadata).HasColumnType("jsonb");
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(m => new { m.DepositId, m.CreatedAt })
            .HasDatabaseName("idx_deposit_messages_deposit_created");

        builder.HasOne(m => m.Deposit)
            .WithMany()
            .HasForeignKey(m => m.DepositId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}