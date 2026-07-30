using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class DepositoRegularizacionConfiguration : IEntityTypeConfiguration<DepositoRegularizacion>
{
    public void Configure(EntityTypeBuilder<DepositoRegularizacion> builder)
    {
        builder.ToTable("deposito_regularizaciones", "public");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(r => r.Accion).HasMaxLength(20).IsRequired();
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()");

        builder.HasOne(r => r.Deposito)
            .WithMany()
            .HasForeignKey(r => r.DepositoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Usuario)
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => new { r.DepositoId, r.CreatedAt });
        builder.HasIndex(r => r.CreatedAt);
    }
}