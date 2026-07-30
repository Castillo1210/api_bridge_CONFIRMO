namespace Confirmo.Api.Models.Entities;

public class DepositoRegularizacion
{
    public Guid Id { get; set; }
    public Guid DepositoId { get; set; }
    public string Accion { get; set; } = string.Empty; // "marcado" | "resuelto" | "desmarcado"
    public Guid? UsuarioId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Motivo { get; set; }
    public string? ImagenAnterior { get; set; }
    public string? ImagenNueva { get; set; }

    public Deposito? Deposito { get; set; }
    public Profile? Usuario { get; set; }
}