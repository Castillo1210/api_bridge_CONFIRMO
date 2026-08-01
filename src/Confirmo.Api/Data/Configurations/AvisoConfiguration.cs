using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class AvisoConfiguration : IEntityTypeConfiguration<Aviso>
{
    public void Configure(EntityTypeBuilder<Aviso> builder)
    {
        builder.ToTable("avisos", "public");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(a => a.Titulo).IsRequired().HasMaxLength(500);
        builder.Property(a => a.MensajeTexto).IsRequired();
        builder.Property(a => a.MediaUrl).HasMaxLength(500);
        builder.Property(a => a.TipoMedia).HasMaxLength(50);

        builder.Property(a => a.RolesDestino)
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'::text[]")
            .IsRequired();

        builder.Property(a => a.AsuntoEmail).HasMaxLength(299);
        builder.Property(a => a.Frecuencia).HasMaxLength(55);
        builder.Property(a => a.Estado).IsRequired().HasMaxLength(55).HasDefaultValue("programado");
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.Activo).HasDefaultValue(true);

        builder.HasOne(a => a.Creador)
            .WithMany()
            .HasForeignKey(a => a.CreadoPor)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.RolesDestino)
            .HasDatabaseName("idx_avisos_roles_destino")
            .HasMethod("gin");

        builder.HasIndex(a => new { a.Estado, a.ProximaEjecucion })
            .HasDatabaseName("idx_avisos_estado_proxima_ejecucion");
    }
}