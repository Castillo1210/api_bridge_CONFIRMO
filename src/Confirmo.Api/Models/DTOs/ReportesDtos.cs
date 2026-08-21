namespace Confirmo.Api.Models.DTOs;

public record ReporteMonedaSummary(
    string Moneda,
    decimal TotalDepositos,
    int CantidadDepositos,
    int DepositosValidados
);

public record ReporteTendenciaDia(string dia, int Confirmados, int Rechazados);

public record ReporteSummaryResponse(List<ReporteMonedaSummary> Summary);

public record ReporteTendenciaResponse(List<ReporteTendenciaDia> Tendencia);