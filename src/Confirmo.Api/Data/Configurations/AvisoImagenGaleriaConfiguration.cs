using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class AvisoImagenGaleriaConfiguration : IEntityTypeConfiguration<AvisoImagenGaleria>
{
    public void Configure(EntityTypeBuilder<AvisoImagenGaleria> builder)
    {
        builder.ToTable("avisos_imagenes_galeria", "public");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(a => a.ObjectName).IsRequired().HasMaxLength(500);
        builder.Property(a => a.ContentType).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Nombre).HasMaxLength(200);
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.Activo).HasDefaultValue(true);

        builder.HasOne(a => a.Creador)
            .WithMany()
            .HasForeignKey(a => a.CreadoPor)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.Activo, a.CreatedAt })
            .HasDatabaseName("idx_avisos_imagenes_galeria_activo_created");
    }
}