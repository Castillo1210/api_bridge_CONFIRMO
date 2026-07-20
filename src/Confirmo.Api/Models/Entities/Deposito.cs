namespace Confirmo.Api.Models.Entities;

public class Deposito
{
    public Guid Id { get; set; }
    public string NumeroOperacion { get; set; } = string.Empty;
    public string? Cuo { get; set; }
    public string? Cliente { get; set; }
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public DateTimeOffset FechaRegistro { get; set; }
    public string? ImagenVoucher { get; set; }
    public string? Anexo { get; set; }
    public string? NumeroOperacionBanco { get; set; }
    public DateOnly? FechaDeposito { get; set; }
    public string Estado { get; set; } = "recibido";
    public string? Observaciones { get; set; }
    public string? MotivoRechazo { get; set; }
    public DateTimeOffset? FechaValidacion { get; set; }
    public Guid? EmpresaId { get; set; }
    public Guid? BancoId { get; set; }
    public Guid? SucursalId { get; set; }
    public Guid VendedorId { get; set; }
    public Guid? ValidadoPor { get; set; }
    public Guid TrabajadorId { get; set; }
    public string? ReferenciaCliente { get; set; }
    public object? DatosOcr { get; set; }
    public string? TelefonoOrigen { get; set; }
    public string? Condicion { get; set; }
    public bool Riesgo { get; set; }
    public string? RucCliente { get; set; }
    public bool? EsAntiguo { get; set; }
    public DateOnly? FechaSoloDate { get; set; }
    public bool PendienteRegularizar { get; set; } = false;
    public Guid[]? ErrorIds { get; set; } = Array.Empty<Guid>();
    public Guid[]? WarningIds { get; set; } = Array.Empty<Guid>();

    // Navigation
    public Empresa? Empresa { get; set; }
    public Sucursal? Sucursal { get; set; }
    public Banco? Banco { get; set; }
    public Profile? Vendedor { get; set; }
    public Trabajador? Trabajador { get; set; }
    public Profile? Validador { get; set; }
}

public class Profile
{
    public Guid Id { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public Guid? SucursalId { get; set; }
    public string Rol { get; set; } = "vendedor";
    public bool Activo { get; set; } = true;
    public string? FcmToken { get; set; }
    public string? DeviceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public Empresa? Empresa { get; set; }
    public Sucursal? Sucursal { get; set; }
}

public static class DepositStates
{
    public const string Recibido = "recibido";
    public const string Procesado = "procesado";
    public const string Rechazado = "rechazado";
    public const string Confirmado = "confirmado";

    public static readonly string[] All = { Recibido, Procesado, Rechazado, Confirmado };
    public static readonly string[] CanReject = { Recibido, Rechazado };
    public static readonly string[] CanConfirm = { Procesado };
    public static readonly string[] CanRegularize = { Rechazado };
}
