public class MovimientoPorIdentificarDto
{
    public int IdOrigen { get; set; }
    public string Cuo { get; set; } = "";
    public string? Cuoa { get; set; }
    public string? Banco { get; set; }
    public string? Cta { get; set; }
    public DateTime? Fecha { get; set; }
    public string? Descripcion { get; set; }
    public string? Plaza { get; set; }
    public string? NroOper { get; set; }
    public double? Cargo { get; set; }
    public double? Abono { get; set; }
    public string? Sd { get; set; }
    public string? Comp { get; set; }
    public string? Tipo { get; set; }
    public string? Agencia { get; set; }
    public string? Ruc { get; set; }
    public string? Razon { get; set; }
    public string? Ubicacion { get; set; }
    public string? Direccion { get; set; }
    public string? Observacion { get; set; }
    public decimal Reg { get; set; }
    public string Registro { get; set; } = "";
    public decimal Dif { get; set; }

    // Datos del deposito emparejado por CUO (puede no haber match)
    public string? Sucursal { get; set; }
    public string? Contacto { get; set; }
    public string? TelefonoContacto { get; set; }
    public string? ValidadoPor { get; set; }
}