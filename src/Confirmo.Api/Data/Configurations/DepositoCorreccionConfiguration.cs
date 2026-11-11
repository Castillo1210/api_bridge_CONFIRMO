using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class DepositoCorreccionConfiguration : IEntityTypeConfiguration<DepositoCorreccion>
{
    public void Configure(EntityTypeBuilder<DepositoCorreccion> builder)
    {
        builder.ToTable("deposito_correcciones", "public");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Accion).HasMaxLength(30).IsRequired();
        builder.Property(c => c.Motivo).HasMaxLength(500).IsRequired();
        builder.Property(c => c.EstadoAnterior).HasMaxLength(20).IsRequired();
        builder.Property(c => c.EstadoNuevo).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Cambios).HasColumnType("jsonb").IsRequired();
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");

        builder.HasOne(c => c.Deposito)
            .WithMany()
            .HasForeignKey(c => c.DepositoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Profile)
            .WithMany()
            .HasForeignKey(c => c.ProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => new { c.DepositoId, c.CreatedAt });
    }
}