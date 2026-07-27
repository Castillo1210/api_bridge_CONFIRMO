using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Endpoints;

public static class SyncStatusEndpoints
{
    public static void MapSyncStatusEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/sync-status", async (
            AppDbContext context,
            CancellationToken cts
        ) =>
        {
            const string sql = @"
                SELECT 'movimientos_bancarios' AS ""Tabla"", empresa AS ""Empresa"",
                    ultima_corrida_en AS ""UltimaCorridaEn"", ultimo_fecha_mod AS ""UltimoFechaMod"",
                    filas_ultima_corrida AS ""FilasUltimaCorrida""
                FROM sync_checkpoints
                UNION ALL
                SELECT 'registros_concar' AS ""Tabla"", empresa AS ""Empresa"",
                    ultima_corrida_en AS ""UltimaCorridaEn"", ultimo_fecha_mod AS ""UltimoFechaMod"",
                    filas_ultima_corrida AS ""FilasUltimaCorrida""
                FROM registros_concar_checkpoints
                ORDER BY ""Tabla"", ""Empresa""";
            
            var estado = await context.Database.SqlQueryRaw<SyncStatusDto>(sql).ToListAsync(cts);

            return Results.Ok(estado);
        })
        .RequireAuthorization()
        .WithName("GetSyncStatus")
        .WithTags("SyncStatus")
        .Produces<List<SyncStatusDto>>(StatusCodes.Status200OK);
    }
}