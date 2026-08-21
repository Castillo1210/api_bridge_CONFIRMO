namespace Confirmo.Api.Models.Entities;

public class ZavuPlantilla
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public Guid CreadoPor { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Profile? Creador { get; set; }
}