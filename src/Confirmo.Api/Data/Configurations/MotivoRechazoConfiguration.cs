using Microsoft.EntityFrameworkCore;
using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class MotivoRechazoConfiguration : IEntityTypeConfiguration<MotivoRechazo>
{
    public void Configure(EntityTypeBuilder<MotivoRechazo> builder)
    {
        builder.ToTable("motivos_rechazo", "public");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.Texto).HasMaxLength(150).IsRequired();
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(m => m.UpdatedAt).HasDefaultValueSql("now()");
        builder.HasIndex(m => m.Orden);
    }
}