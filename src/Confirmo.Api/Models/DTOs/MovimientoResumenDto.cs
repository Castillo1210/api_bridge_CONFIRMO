namespace Confirmo.Api.Models.DTOs;

public class MovimientoResumenDto
{
    public DateTime? Fecha { get; set; }
    public string? Banco { get; set; }
    public string? NroOper { get; set; }
    public string? Descripcion { get; set; }
    public double? Abono { get; set; }
}