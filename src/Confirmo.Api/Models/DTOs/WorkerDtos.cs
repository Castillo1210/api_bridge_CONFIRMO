namespace Confirmo.Api.Models.DTOs;

public record WorkerResult(
    string DepositId,
    string Status,
    List<string> ErrorIds,
    List<string> WarningIds,
    string? ErrorType,
    string? ErrorMessage
);

public record ProcessDepositMessage(
    string DepositId,
    string ObjectName,
    string? BancoId,
    string EmpresaId,
    string? Cliente,
    int RetryCount = 0
);