namespace Confirmo.Api.Models.DTOs;

public record DepositCreateRequest(
    string NumeroOperacion,
    string? Cliente,
    decimal Monto,
    string Moneda,
    DateOnly? FechaDeposito,
    string? BancoId,
    string? Cuenta,
    string? Anexo,
    string ImagenBase64,
    string? ReferenciaCliente,
    string? RucCliente
);

public record DepositResponse(
    Guid Id,
    string NumeroOperacion,
    string? Cliente,
    decimal Monto,
    string Moneda,
    DateTimeOffset FechaRegistro,
    string? ImagenVoucher,
    string? Anexo,
    string? NumeroOperacionBanco,
    DateOnly? FechaDeposito,
    string Estado,
    string? Observaciones,
    string? MotivoRechazo,
    DateTimeOffset? FechaValidacion,
    Guid EmpresaId,
    Guid? BancoId,
    Guid? SucursalId,
    Guid VendedorId,
    string? ReferenciaCliente,
    object? DatosOcr,
    string? RucCliente
);

public record DepositListResponse(
    Guid Id,
    string NumeroOperacion,
    string? Cliente,
    decimal Monto,
    string Moneda,
    DateTimeOffset FechaRegistro,
    string Estado,
    string? NumeroOperacionBanco,
    DateOnly? FechaDeposito
);

public record DepositListPagedResponse(
    List<DepositListResponse> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record BancoResponse(
    Guid Id,
    string Nombre,
    string? Codigo
);