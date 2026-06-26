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
        builder.Property(d => d.ImagenVoucher).HasMaxLength(100);
        builder.Property(d => d.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("pendiente");
        builder.Property(d => d.Observaciones).HasMaxLength(1000);
        builder.Property(d => d.MotivoRechazo).HasMaxLength(500);
        builder.Property(d => d.ReferenciaCliente).HasMaxLength(100);
        builder.Property(d => d.DatosOcr).HasColumnType("jsonb");
        builder.Property(d => d.RucCliente).HasMaxLength(20);
        builder.Property(d => d.TelefonoOrigen).HasMaxLength(20);

        // Indices
        builder.HasIndex(d => new { d.VendedorId, d.FechaRegistro }).HasDatabaseName("idx_depositos_vendedor_fecha");
        builder.HasIndex(d => new { d.Estado, d.FechaRegistro }).HasDatabaseName("idx_depositos_estado_fecha");
        builder.HasIndex(d => new { d.EmpresaId, d.Estado }).HasDatabaseName("idx_depositos_empresa_estado");
        builder.HasIndex(d => d.NumeroOperacionBanco).IsUnique().HasDatabaseName("uk_depositos_numero_operacion_banco").HasFilter("numero_operacion_banco IS NOT NULL");

        // En DepositoConfiguration.Configure() - AGREGAR al final:

        // Campos para integración Worker
        builder.Property(d => d.ErrorIds)
            .HasColumnType("uuid[]")
            .HasDefaultValueSql("'{}'::uuid[]");

        builder.Property(d => d.WarningIds)
            .HasColumnType("uuid[]")
            .HasDefaultValueSql("'{}'::uuid[]");

        // Índices para consultas frecuentes
        builder.HasIndex(d => d.ErrorIds)
            .HasDatabaseName("idx_depositos_error_ids")
            .HasMethod("gin");  // GIN index para array UUID

        builder.HasIndex(d => d.WarningIds)
            .HasDatabaseName("idx_depositos_warning_ids")
            .HasMethod("gin");
    }
}

public class BancoConfiguration : IEntityTypeConfiguration<Banco>
{
    public void Configure(EntityTypeBuilder<Banco> builder)
    {
        builder.ToTable("bancos", "public");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Codigo).HasMaxLength(20);
    }
}

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresas", "public");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Ruc).HasMaxLength(20);
    }
}

public class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("sucursales", "public");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Direccion).HasMaxLength(500);
    }
}

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles", "public");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PhoneNumber).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.PhoneNumber).IsUnique();
        builder.Property(p => p.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(p => p.FullName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Rol).HasMaxLength(55);
        builder.Property(p => p.FcmToken).HasMaxLength(500);
    }
}