namespace Confirmo.Api.Models.DTOs;

public record DepositConfirmedNotification(
    Guid DepositId,
    string Estado,
    string ReferenceNumber,
    string Empresa,
    string Sucursal,
    string Banco,
    string Anexo,
    DateOnly FechaDeposito,
    string NumeroOperacion,
    string Importe,
    string Moneda
);

public record ConfirmDepositRequest(
    string? Observaciones
);

public record ConfirmDepositResponse(
    bool Success,
    string Message,
    DepositConfirmedNotification? Notification
);

// Panel Voucher (nuevo)
public record VoucherProcessUpdate(
    Guid DepositId,
    int Progress,
    string Stage,
    string? Message
);

public record VoucherOcrResult(
    Guid DepositId,
    string? NumeroOperacion,
    string? Banco,
    decimal? Monto,
    string? Moneda,
    DateOnly? Fecha,
    float Confidence
);

public record VoucherErrorNotification(
    Guid DepositId,
    string ErrorCode,
    string Title,
    string Message,
    string? UserAction
);

// Panel Finanzas
public record PanelDepositSummary(
    Guid DepositId,
    string NumeroOperacion,
    string? Cliente,
    decimal Monto,
    string Moneda,
    string Estado,
    DateTimeOffset FechaRegistro,
    string? Banco,
    string? Sucursal,
    string? VendedorNombre
);

public record PanelStatsUpdate(
    int Pendientes,
    int Recibidos,
    int EnProceso,
    int Validados,
    int RequierenRevision,
    int Rechazados,
    int Confirmados,
    int TotalHoy
);

// Sistema
public record ConnectionStatusUpdate(
    string Status,
    DateTimeOffset Timestamp,
    string? Message
);

public record SystemAlert(
    string AlertId,
    string Severity,
    string Title,
    string Message,
    DateTimeOffset Timestamp
);