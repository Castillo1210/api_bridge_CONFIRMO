namespace Confirmo.Api.Models.Entities;

public class CuentaBancaria
{
    public Guid Id { get; set; }
    public string NumeroCuenta { get; set; } = string.Empty;
    public string Anexo { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public Guid BancoId { get; set; }
    public bool Activo { get; set; }

    // Navigation
    public Empresa? Empresa { get; set; }
    public Banco? Banco { get; set; }
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
    public string? Logo { get; set; }
    public bool Activo { get; set; } = true;
}

public class Sucursal
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public bool Activo { get; set; }

    public Empresa?  Empresa { get; set; }
}

public class Trabajador
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? TelefonoPersonal { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid? SucursalId { get; set; }
    public bool Activo { get; set; } = true;
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public Guid CreadoPor { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Profile? Profile { get; set; }
    public Empresa? Empresa { get; set; }
    public Sucursal? Sucursal { get; set; }
    public Profile? Creador { get; set; }
}