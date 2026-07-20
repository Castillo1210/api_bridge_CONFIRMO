namespace Confirmo.Api.Models.DTOs;

public class MovimientoResumenDto
{
    public int IdOrigen { get; set; }
    public DateTime? Fecha { get; set; }
    public string? Banco { get; set; }
    public string? NroOper { get; set; }
    public string? Descripcion { get; set; }
    public double? Abono { get; set; }
}

public record MarcarTipoRequest(string Empresa, int IdOrigen, string Tipo, Guid? DepositId);