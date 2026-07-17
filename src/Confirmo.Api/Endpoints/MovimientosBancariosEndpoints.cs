using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Endpoints;

public static class MovimientosBancariosEndpoints
{
    private static readonly string[] EmpresasValidas = { "JCH", "EVO" };

    public static void MapMovimientosBancariosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/movimientos-bancarios")
            .RequireAuthorization()
            .WithTags("MovimientosBancarios");

        group.MapGet("/", async (
            string empresa,
            DateOnly fechaDesde,
            DateOnly fechaHasta,
            AppDbContext context,
            CancellationToken cts
        ) =>
        {
            var empresaNormalizada = empresa?.Trim().ToUpperInvariant() ?? "";

            if (!EmpresasValidas.Contains(empresaNormalizada))
            {
                return Results.BadRequest($"Empresa invalida. Valores permitidos: {string.Join(", ", EmpresasValidas)}");
            }

            if (fechaHasta < fechaDesde)
            {
                return Results.BadRequest("fechaHasta no puede ser anterior a fechaDesde");
            }

            if ((fechaHasta.ToDateTime(TimeOnly.MinValue) - fechaDesde.ToDateTime(TimeOnly.MinValue)).TotalDays > 62)
            {
                return Results.BadRequest("El rango de fechas no puede superar 62 días");
            }

            var desde = fechaDesde.ToDateTime(TimeOnly.MinValue);
            var hastaExclusivo = fechaHasta.ToDateTime(TimeOnly.MinValue).AddDays(1);

            const string sql = @"
                SELECT
                    fecha AS ""Fecha"",
                    banco AS ""Banco"",
                    nro_oper AS ""NroOper"",
                    descripcion AS ""Descripcion"",
                    abono AS ""Abono""
                FROM movimientos_bancarios
                WHERE empresa = {0}
                    AND fecha >= {1}
                    AND fecha < {2}
                    AND abono > 0
                ORDER BY fecha ASC";

            var movimientos = await context.Database.SqlQueryRaw<MovimientoResumenDto>(sql, empresaNormalizada, desde, hastaExclusivo).ToListAsync(cts);

            return Results.Ok(movimientos);
        })
        .WithName("GetMovimientosBancarios")
        .Produces<List<MovimientoResumenDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}