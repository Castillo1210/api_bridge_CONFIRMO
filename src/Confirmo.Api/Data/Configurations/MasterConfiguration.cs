using Microsoft.EntityFrameworkCore;
using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Confirmo.Api.Data.Configurations;

public class CuentaBancariaConfiguration : IEntityTypeConfiguration<CuentaBancaria>
{
    public void Configure(EntityTypeBuilder<CuentaBancaria> builder)
    {
        builder.ToTable("cuentasbancarias", "public");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.NumeroCuenta).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Anexo).IsRequired().HasMaxLength(25);

        builder.HasOne(c => c.Empresa).WithMany().HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Banco).WithMany().HasForeignKey(c => c.BancoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BancoConfiguration : IEntityTypeConfiguration<Banco>
{
    public void Configure(EntityTypeBuilder<Banco> builder)
    {
        builder.ToTable("bancos", "public");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id).HasDefaultValueSql("gen_random_uuid()");
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

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Logo).HasMaxLength(500);
        builder.Property(e => e.Ruc).HasMaxLength(20);
    }
}

public class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("sucursales", "public");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Direccion).HasMaxLength(500);

        builder.HasOne(s => s.Empresa).WithMany().HasForeignKey(s => s.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrabajadorConfiguration : IEntityTypeConfiguration<Trabajador>
{
    public void Configure(EntityTypeBuilder<Trabajador> builder)
    {
        builder.ToTable("trabajadores", "public");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.Nombre).IsRequired().HasMaxLength(255);
        builder.Property(t => t.TelefonoPersonal).HasMaxLength(55);
        builder.Property(t => t.FechaInicio).HasDefaultValueSql("now()");

        builder.HasOne(t => t.Profile).WithMany().HasForeignKey(t => t.ProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Empresa).WithMany().HasForeignKey(t => t.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Sucursal).WithMany().HasForeignKey(t => t.SucursalId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => new { t.ProfileId, t.Activo })
            .HasDatabaseName("idx_trabajadores_profile_activo");
        builder.HasIndex(t => new { t.EmpresaId, t.SucursalId })
            .HasDatabaseName("idx_trabajadores_empresa_sucursal");
        builder.HasOne(t => t.Creador).WithMany().HasForeignKey(t => t.CreadoPor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}