namespace Confirmo.Api.Models.Entities;

public class DepositoEvento
{
    public long Id { get; set; }
    public Guid DepositoId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string Evento { get; set; } = string.Empty;
    public string Origen { get; set; } = "servidor";
    public Guid? SesionId { get; set; }
    public string? Metadata { get; set; }
    public DateTimeOffset OcurridoEn { get; set; }
    public DateTimeOffset RegistradoEn { get; set; }

    public Deposito? Deposito { get; set; }
    public Profile? Usuario { get; set; }
}