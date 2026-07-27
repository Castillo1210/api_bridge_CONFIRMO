namespace Confirmo.Api.Models.DTOs;

public class CortadoDto
{
    public int IdOrigen { get; set; }
    public string Cuo { get; set; } = "";
    public string? Periodo { get; set; }
    public string? Banco { get; set; }
    public DateTime? Fecha { get; set; }
    public string? Descripcion { get; set; }
    public string? NroOper { get; set; }
    public double? Cargo { get; set; }
    public double? Abono { get; set; }
    public string? Sd { get; set; }
    public string? Comp { get; set; }
    public string? Tipo { get; set; }
    public string? Doc { get; set; }
    public string? Area { get; set; }
    public string? Observacion { get; set; }
    public string Registro { get; set; } = "";
    public string Glosa { get; set; } = "";
    public decimal Reg { get; set; }
    public decimal Dif { get; set; }
    public long TotalCount { get; set; }
}