using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class DepositoRechazoHistorialConfiguration : IEntityTypeConfiguration<DepositoRechazoHistorial>
{
    public void Configure(EntityTypeBuilder<DepositoRechazoHistorial> builder)
    {
        builder.ToTable("deposito_rechazos_historial", "public");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(r => r.MotivoRechazo).HasMaxLength(500);

        // No hay HasIndex(...).IsUnique() por DepositoId --  cada rechazo regularizado agrega una fila nueva
        builder.HasOne(r => r.Deposito)
            .WithMany()
            .HasForeignKey(r => r.DepositoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Rechazador)
            .WithMany()
            .HasForeignKey(r => r.RechazadoPor)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Regularizador)
            .WithMany()
            .HasForeignKey(r => r.RegularizadoPor)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => new { r.DepositoId, r.CreatedAt });
    }
}