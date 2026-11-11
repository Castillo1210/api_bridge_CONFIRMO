namespace Confirmo.Api.Models.DTOs;

public record CorregirDepositoRequest(
    string Motivo,
    string? Anexo,
    string? NumeroOperacion,
    string? EmpresaId,
    string? BancoId,
    decimal? Monto,
    string? Moneda,
    string? FechaDeposito,
    string? Cliente,
    string? NumeroTarjeta
);

public record RevertirDepositoRequest(string Motivo, string Destino);

public record CambioCampoDto(string Campo, string? Antes, string? Despues);

public record DepositoCorreccionDto(
    Guid Id,
    Guid DepositoId,
    string Accion,
    string Motivo,
    string EstadoAnterior,
    string EstadoNuevo,
    string Cambios,
    string? UsuarioNombre,
    DateTimeOffset CreatedAt
);