using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class DepositoEventoConfiguration : IEntityTypeConfiguration<DepositoEvento>
{
    public void Configure(EntityTypeBuilder<DepositoEvento> builder)
    {
        builder.ToTable("deposito_eventos", "public");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityAlwaysColumn();
        builder.Property(e => e.Evento).HasMaxLength(40).IsRequired();
        builder.Property(e => e.Origen).HasMaxLength(40).IsRequired();
        builder.Property(e => e.Metadata).HasColumnType("jsonb");
        builder.Property(e => e.RegistradoEn).HasDefaultValueSql("now()");

        builder.HasOne(e => e.Deposito).WithMany().HasForeignKey(e => e.DepositoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.DepositoId, e.OcurridoEn });
        builder.HasIndex(e => new { e.UsuarioId, e.OcurridoEn });
        builder.HasIndex(e => new { e.Evento, e.OcurridoEn });
    }
}