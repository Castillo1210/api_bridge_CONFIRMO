namespace Confirmo.Api.Models.Entities;

public class DepositoCorreccion
{
    public Guid Id { get; set; }
    public Guid DepositoId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public Guid? ProfileId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;

    public string Cambios { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; }

    public Deposito? Deposito { get; set; }
    public Profile? Profile { get; set; }
}