using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class DepositoConfiguration : IEntityTypeConfiguration<Deposito>
{
    public void Configure(EntityTypeBuilder<Deposito> builder)
    {
        builder.ToTable("depositos", "public");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(d => d.NumeroOperacion).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Cliente).HasMaxLength(200);
        builder.Property(d => d.Monto).HasPrecision(12, 2).IsRequired();
        builder.Property(d => d.Moneda).IsRequired().HasMaxLength(3);
        builder.Property(d => d.FechaRegistro).HasDefaultValueSql("now()");
        builder.Property(d => d.ImagenVoucher).HasMaxLength(500);
        builder.Property(d => d.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("recibido");
        builder.Property(d => d.Observaciones).HasMaxLength(1000);
        builder.Property(d => d.Condicion).HasMaxLength(100);
        builder.Property(d => d.MotivoRechazo).HasMaxLength(500);
        builder.Property(d => d.ReferenciaCliente).HasMaxLength(100);
        builder.Property(d => d.DatosOcr).HasColumnType("jsonb");
        builder.Property(d => d.RucCliente).HasMaxLength(20);
        builder.Property(d => d.TelefonoOrigen).HasMaxLength(20);

        // Indices
        builder.HasIndex(d => new { d.VendedorId, d.FechaRegistro }).HasDatabaseName("idx_depositos_vendedor_fecha");
        builder.HasIndex(d => new { d.Estado, d.FechaRegistro }).HasDatabaseName("idx_depositos_estado_fecha");
        builder.HasIndex(d => new { d.EmpresaId, d.Estado }).HasDatabaseName("idx_depositos_empresa_estado");

        // En DepositoConfiguration.Configure() - AGREGAR al final:

        // FKs con Restrict para evitar borrados en cascada
        builder.HasOne(d => d.Empresa).WithMany().HasForeignKey(d => d.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Banco).WithMany().HasForeignKey(d => d.BancoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Sucursal).WithMany().HasForeignKey(d => d.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Vendedor).WithMany().HasForeignKey(d => d.VendedorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Validador).WithMany().HasForeignKey(d => d.ValidadoPor)
            .OnDelete(DeleteBehavior.Restrict);

        // Campos para integración Worker
        builder.Property(d => d.ErrorIds)
            .HasColumnType("uuid[]")
            .HasDefaultValueSql("'{}'::uuid[]");

        builder.Property(d => d.WarningIds)
            .HasColumnType("uuid[]")
            .HasDefaultValueSql("'{}'::uuid[]");

        builder.Property(d => d.TrabajadorId).IsRequired().HasColumnName("trabajador_id");
        builder.HasOne(d => d.Trabajador).WithMany().HasForeignKey(d => d.TrabajadorId).OnDelete(DeleteBehavior.Restrict);

        // Índices para consultas frecuentes
        builder.HasIndex(d => d.ErrorIds)
            .HasDatabaseName("idx_depositos_error_ids")
            .HasMethod("gin");  // GIN index para array UUID

        builder.HasIndex(d => d.WarningIds)
            .HasDatabaseName("idx_depositos_warning_ids")
            .HasMethod("gin");
    }
}

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles", "public");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PhoneNumber).HasMaxLength(100);
        builder.HasIndex(p => p.PhoneNumber).IsUnique();
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.HasIndex(p => p.Email).IsUnique().HasFilter("email IS NOT NULL");
        builder.Property(p => p.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(p => p.FullName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Rol).HasMaxLength(55);
        builder.Property(p => p.FcmToken).HasMaxLength(500);
        builder.Property(p => p.DeviceId).HasMaxLength(200);

        builder.HasOne(p => p.Empresa).WithMany().HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Sucursal).WithMany().HasForeignKey(p => p.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}