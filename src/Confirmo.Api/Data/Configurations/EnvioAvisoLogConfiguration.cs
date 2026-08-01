using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class EnvioAvisoLogConfiguration : IEntityTypeConfiguration<EnvioAvisoLog>
{
    public void Configure(EntityTypeBuilder<EnvioAvisoLog> builder)
    {
        builder.ToTable("envio_aviso_logs", "public");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Canal).IsRequired().HasMaxLength(55);
        builder.Property(e => e.Estado).IsRequired().HasMaxLength(55);
        builder.Property(e => e.ZavuMessageId).HasMaxLength(200);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

        builder.HasOne(e => e.Aviso)
            .WithMany()
            .HasForeignKey(e => e.AvisoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Profile)
            .WithMany()
            .HasForeignKey(e => e.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(e => new { e.AvisoId, e.ProfileId, e.Canal });
    }
}