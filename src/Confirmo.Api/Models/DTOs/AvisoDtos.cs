namespace Confirmo.Api.Models.DTOs;

public record CreateAvisoRequest(
    string Titulo,
    string MensajeTexto,
    string? MediaUrl,
    string? TipoMedia,
    string[] RolesDestino,
    bool EnviarApp,
    bool EnviarWhatsapp,
    bool EnviarEmail,
    string? AsuntoEmail,
    bool EsRecurrente,
    string? Frecuencia,
    TimeSpan? HoraEjecucion,
    int? DiaSemana,
    int? DiaMes,
    DateTimeOffset? ProgramadoPara
);

public record AvisoResponse(
    Guid Id,
    string Titulo,
    string MensajeTexto,
    string? MediaUrl,
    string? TipoMedia,
    string[] RolesDestino,
    bool EnviarApp,
    bool EnviarWhatsapp,
    bool EnviarEmail,
    string? AsuntoEmail,
    bool EsRecurrente,
    string? Frecuencia,
    DateTimeOffset? ProximaEjecucion,
    DateTimeOffset? UltimaEjecucion,
    string Estado,
    bool Activo,
    string? CreadoPorNombre,
    DateTimeOffset CreatedAt
);

public record UploadAvisoMediaRequest(string ImagenBase64, string? ContentType);