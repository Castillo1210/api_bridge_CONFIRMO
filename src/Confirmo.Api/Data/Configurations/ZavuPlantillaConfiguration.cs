using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class ZavuPlantillaConfiguration : IEntityTypeConfiguration<ZavuPlantilla>
{
    public void Configure(EntityTypeBuilder<ZavuPlantilla> builder)
    {
        builder.ToTable("zavu_plantillas", "public");
        builder.HasKey(z => z.Id);
        builder.Property(z => z.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(z => z.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(z => z.Codigo).IsRequired().HasMaxLength(100);
        builder.Property(z => z.TemplateId).IsRequired().HasMaxLength(200);
        builder.Property(z => z.Activo).HasDefaultValue(true);
        builder.Property(z => z.CreatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(z => z.Codigo).IsUnique();

        builder.HasOne(z => z.Creador)
            .WithMany()
            .HasForeignKey(z => z.CreadoPor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}