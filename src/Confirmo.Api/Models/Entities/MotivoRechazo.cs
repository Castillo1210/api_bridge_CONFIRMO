namespace Confirmo.Api.Models.Entities;

public class MotivoRechazo
{
    public Guid Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}