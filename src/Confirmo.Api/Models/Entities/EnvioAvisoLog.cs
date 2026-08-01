namespace Confirmo.Api.Models.Entities;

public class EnvioAvisoLog
{
    public Guid Id { get; set; }
    public Guid AvisoId { get; set; }
    public Guid ProfileId { get; set; }
    public string Canal { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? ZavuMessageId { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Aviso? Aviso { get; set; }
    public Profile? Profile { get; set; }
}