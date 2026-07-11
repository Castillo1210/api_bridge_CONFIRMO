namespace Confirmo.Api.Models.Entities;

public class PlantillaMensajeSistema
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public string Canal { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}