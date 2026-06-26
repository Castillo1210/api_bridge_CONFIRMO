using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class VoucherBusinessErrorConfiguration : IEntityTypeConfiguration<VoucherBusinessError>
{
    public void Configure(EntityTypeBuilder<VoucherBusinessError> builder)
    {
        builder.ToTable("voucher_business_errors", "public");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(e => e.ErrorCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.ErrorCode).IsUnique();  // Unique constraint
        
        builder.Property(e => e.FieldName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Severity).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Message).IsRequired().HasColumnType("text");
        builder.Property(e => e.UserAction).HasColumnType("text");
        
        builder.Property(e => e.IsActive).HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        
        builder.HasIndex(e => new { e.IsActive, e.Severity })
            .HasDatabaseName("idx_voucher_business_errors_active_severity");
    }
}