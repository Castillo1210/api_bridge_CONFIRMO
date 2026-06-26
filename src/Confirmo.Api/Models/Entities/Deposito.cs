namespace Confirmo.Api.Models.Entities;

public class Deposito
{
    public Guid Id { get; set; }
    public string NumeroOperacion { get; set; } = string.Empty;
    public string? Cliente { get; set; }
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public DateTimeOffset FechaRegistro { get; set; }
    public string? ImagenVoucher { get; set; }
    public string? Anexo { get; set; }
    public string? NumeroOperacionBanco { get; set; }
    public DateOnly? FechaDeposito { get; set; }
    public string Estado { get; set; } = "pendiente";
    public string? Observaciones { get; set; }
    public string? MotivoRechazo { get; set; }
    public DateTimeOffset? FechaValidacion { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid? BancoId { get; set; }
    public Guid? SucursalId { get; set; }
    public Guid VendedorId { get; set; }
    public Guid? ValidadoPor { get; set; }
    public long? TrabajadorSucursalId { get; set; }
    public string? ReferenciaCliente { get; set; }
    public object? DatosOcr { get; set; }
    public string? TelefonoOrigen { get; set; }
    public string? RucCliente { get; set; }
    public bool? EsAntiguo { get; set; }
    public DateOnly? FechaSoloDate { get; set; }
    public Guid[] ErrorIds { get; set; } = Array.Empty<Guid>();
    public Guid[] WarningIds { get; set; } = Array.Empty<Guid>();
}

public class Banco
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public bool Activo { get; set; } = true;
}

public class Empresa
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Ruc { get; set; }
    public bool Activo { get; set; } = true;
}

public class Sucursal
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public bool Activo { get; set; }
}

public class Profile
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public Guid? SucursalId { get; set; }
    public string Rol { get; set; } = "vendedor";
    public bool Activo { get; set; } = true;
    public string? FcmToken { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
}