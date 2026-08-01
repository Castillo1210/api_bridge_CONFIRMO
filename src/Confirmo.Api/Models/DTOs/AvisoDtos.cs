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
    TimeSpan? HoraEjecucion,
    int? DiaSemana,
    int? DiaMes,
    DateTimeOffset? ProximaEjecucion,
    DateTimeOffset? UltimaEjecucion,
    string Estado,
    bool Activo,
    string? CreadoPorNombre,
    DateTimeOffset CreatedAt
);

public record UpdateAvisoRequest(
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

public record ReenviarAvisoRequest(DateTimeOffset? ProgramadoPara);

public record AvisoImagenGaleriaResponse(
    Guid Id,
    string ObjectName,
    string ContentType,
    string? Nombre,
    DateTimeOffset CreatedAt
);

public record UploadAvisoMediaRequest(string ImagenBase64, string? ContentType, string? Nombre = null);