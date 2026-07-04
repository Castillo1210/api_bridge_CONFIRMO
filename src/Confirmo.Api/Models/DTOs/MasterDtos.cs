namespace Confirmo.Api.Models.DTOs;

public record BancoResponse(
    Guid Id,
    string Nombre,
    string? Codigo
);

public record EmpresaResponse(
    Guid Id,
    string Nombre,
    string? Logo
);

public record SucursalResponse(
    Guid Id,
    Guid? EmpresaId,
    string Nombre,
    string? Direccion,
    bool Activo
);

public record CuentaBancariaResponse(
    Guid Id,
    Guid? EmpresaId,
    Guid? BancoId,
    string NumeroCuenta,
    string Anexo,
    bool Activo
);

public record TrabajadorResponse(
    Guid Id,
    Guid ProfileId,
    string Nombre,
    string? TelefonoPersonal,
    Guid EmpresaId,
    Guid? SucursalId,
    bool Activo,
    DateOnly FechaInicio,
    DateOnly? FechaFin
);

// Bancos
public record CreateBancoRequest(string Nombre, string? Codigo);
public record UpdateBancoRequest(string Nombre, string? Codigo, bool Activo);

// Empresas
public record CreateEmpresaRequest(string Nombre, string? Ruc, string? Logo);
public record UpdateEmpresaRequest(string Nombre, string? Ruc, string? Logo, bool Activo);

// Sucursales
public record CreateSucursalRequest(Guid EmpresaId, string Nombre, string? Direccion);
public record UpdateSucursalRequest(Guid EmpresaId, string Nombre, string? Direccion, bool Activo);

// Cuentas Bancarias
public record CreateCuentaBancariaRequest(string NumeroCuenta, string Anexo, Guid EmpresaId, Guid BancoId);
public record UpdateCuentaBancariaRequest(string NumeroCuenta, string Anexo, Guid EmpresaId, Guid BancoId, bool Activo);

// Trabajadores
public record CreateTrabajadorRequest(
    Guid ProfileId,
    string Nombre,
    string? TelefonoPersonal,
    Guid EmpresaId,
    Guid? SucursalId,
    DateOnly FechaInicio
);

public record UpdateTrabajadorRequest(
    string Nombre,
    string? TelefonoPersonal,
    Guid? SucursalId,
    bool Activo
);

// Profiles
public record ProfileResponse(
    Guid Id,
    string? PhoneNumber,
    string? Email,
    string FullName,
    Guid EmpresaId,
    Guid? SucursalId,
    string Rol,
    bool Activo,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt
);

public record CreateProfileRequest(
    string? PhoneNumber,
    string? Email,
    string Password,
    string FullName,
    Guid EmpresaId,
    Guid? SucursalId,
    string Rol
);

public record UpdateProfileRequest(
    string? PhoneNumber,
    string? Email,
    string FullName,
    Guid EmpresaId,
    Guid? SucursalId,
    string Rol,
    bool Activo
);

public record ResetProfilePasswordRequest(
    string NewPassword
);