using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Confirmo.Api.Data.Configurations;

public class PlantillaMensajeSistemaConfiguration : IEntityTypeConfiguration<PlantillaMensajeSistema>
{
    public void Configure(EntityTypeBuilder<PlantillaMensajeSistema> builder)
    {
        builder.ToTable("plantillas_mensajees_sistema", "public");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(60);
        builder.HasIndex(p => p.Codigo).IsUnique();

        builder.Property(p => p.Contenido).IsRequired().HasColumnType("text");
        builder.Property(p => p.Descripcion).HasMaxLength(200);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("now()");
    }
}