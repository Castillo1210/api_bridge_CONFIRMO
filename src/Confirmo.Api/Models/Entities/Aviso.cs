namespace Confirmo.Api.Models.Entities;

public class Aviso
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string MensajeTexto { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public string? TipoMedia { get; set; }
    public string[] RolesDestino { get; set; } = Array.Empty<string>();
    public bool EnviarApp { get; set; } = true;
    public bool EnviarWhatsapp { get; set; } = false;
    public bool EnviarEmail { get; set; } = false;
    public string? AsuntoEmail { get; set; }
    public bool EsRecurrente { get; set; } = false;
    public string? Frecuencia { get; set; }
    public TimeSpan? HoraEjecucion { get; set; }
    public int? DiaSemana { get; set; }
    public int? DiaMes { get; set; }
    public DateTimeOffset? ProximaEjecucion { get; set; }
    public DateTimeOffset? UltimaEjecucion { get; set; }
    public Guid CreadoPor { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Estado { get; set; } = "programado";
    public bool Activo { get; set; } = true;

    public Profile? Creador { get; set; }
}