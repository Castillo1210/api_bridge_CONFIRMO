namespace Confirmo.Api.Models.Entities;

public class AvisoImagenGaleria
{
    public Guid Id { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? Nombre { get; set; }
    public Guid CreadoPor { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool Activo { get; set; } = true;

    public Profile? Creador { get; set; }
}