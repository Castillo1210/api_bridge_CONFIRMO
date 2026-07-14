namespace Confirmo.Api.Models.DTOs;

public record DepositCreateRequest(
    string? Cliente,
    string? EmpresaId,
    string? BancoId,
    string ImagenBase64
);

public record RegularizeDepositRequest(
    string ImagenBase64,
    string? Cliente,
    string? BancoId,
    string? EmpresaId
);

public record DepositResponse(
    Guid Id,
    string NumeroOperacion,
    string? Cliente,
    decimal Monto,
    string Moneda,
    DateTimeOffset FechaRegistro,
    string? ImagenVoucher,
    string? ImagenUrl,
    string? Anexo,
    string? NumeroOperacionBanco,
    DateOnly? FechaDeposito,
    string Estado,
    string? Observaciones,
    string? MotivoRechazo,
    bool Riesgo,
    DateTimeOffset? FechaValidacion,
    Guid? ValidadoPor,
    Guid? EmpresaId,
    Guid? BancoId,
    Guid? SucursalId,
    Guid VendedorId,
    Guid TrabajadorId,
    string? ReferenciaCliente,
    object? DatosOcr,
    string? RucCliente,

    EmpresaResponse? Empresa = null,
    BancoResponse? Banco = null,
    SucursalResponse? Sucursal = null,
    TrabajadorResponse? Trabajador = null
);

public record DepositListResponse(
    Guid Id,
    string NumeroOperacion,
    string? Cliente,
    decimal Monto,
    string Moneda,
    DateTimeOffset FechaRegistro,
    string Estado,
    string? Condicion,
    bool Riesgo,
    string? NumeroOperacionBanco,
    DateOnly? FechaDeposito,
    Guid? SucursalId,
    Guid? BancoId,
    Guid? EmppresaId,
    Guid? TrabajadorId,
    Guid? ValidadoPor,
    EmpresaResponse? Empresa = null,
    BancoResponse? Banco = null
);

public record DepositListPagedResponse(
    List<DepositListResponse> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record CheckDuplicateRequest(
    decimal Monto,
    string Moneda,
    string NumeroOperacion,
    Guid? ExcludeId
);