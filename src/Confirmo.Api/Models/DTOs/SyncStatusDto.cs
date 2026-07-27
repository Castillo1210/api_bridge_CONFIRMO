namespace Confirmo.Api.Models.DTOs;

public class SyncStatusDto
{
    public string Tabla { get; set; } = "";
    public string Empresa { get; set; } = "";
    public DateTime? UltimaCorridaEn { get; set; }
    public DateTime? UltimoFechaMod { get; set; }
    public int FilasUltimaCorrida { get; set; }
}