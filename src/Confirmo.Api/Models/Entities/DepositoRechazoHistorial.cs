namespace Confirmo.Api.Models.Entities;

public class DepositoRechazoHistorial
{
    public Guid Id { get; set; }
    public Guid DepositoId { get; set; }
    public string? ImagenVoucherRechazada { get; set; }
    public string? MotivoRechazo { get; set; }
    public string? Observaciones { get; set; }
    public DateTimeOffset? FechaRechazo { get; set; }
    public Guid? RechazadoPor { get; set; }
    public Guid? RegularizadoPor { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Deposito? Deposito { get; set; }
    public Profile? Rechazador { get; set; }
    public Profile? Regularizador { get; set; }
}