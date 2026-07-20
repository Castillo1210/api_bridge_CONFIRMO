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
            string? search,
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

            var desde = DateTime.SpecifyKind(fechaDesde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var hastaExclusivo = DateTime.SpecifyKind(fechaHasta.ToDateTime(TimeOnly.MinValue).AddDays(1), DateTimeKind.Utc);
            var searchPattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";

            const string sql = @"
                SELECT
                    id_origen AS ""IdOrigen"",
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
                    AND ({3}::text IS NULL OR nro_oper ILIKE {3} OR banco ILIKE {3} OR descripcion ILIKE {3})
                ORDER BY fecha ASC";

            var movimientos = await context.Database.SqlQueryRaw<MovimientoResumenDto>(sql, empresaNormalizada, desde, hastaExclusivo, searchPattern!).ToListAsync(cts);

            return Results.Ok(movimientos);
        })
        .WithName("GetMovimientosBancarios")
        .Produces<List<MovimientoResumenDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/marcar-tipo", async (
            MarcarTipoRequest request,
            AppDbContext context,
            CancellationToken cts
        ) =>
        {
            var empresaNormalizada = request.Empresa?.Trim().ToUpperInvariant() ?? "";

            if (!EmpresasValidas.Contains(empresaNormalizada))
            {
                return Results.BadRequest($"Empresa invalida. Valores permitidos {string.Join(", ", EmpresasValidas)}");
            }

            if (request.IdOrigen <= 0)
            {
                return Results.BadRequest("idOrigen invalido");
            }

            var tipo = request.Tipo?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(tipo))
            {
                return Results.BadRequest("Tipo no puede estar vació");
            }

            if (tipo.Length > 200)
            {
                tipo = tipo[..200];
            }

            const string sql = @"
                UPDATE movimientos_bancarios
                SET tipo = {2}
                WHERE empresa = {0} AND id_origen = {1};
                
                INSERT INTO movimientos_tipo_pendientes (empresa, id_origen, tipo, deposito_id)
                VALUES ({0}, {1}, {2}, {3})
                ON CONFLICT (empresa, id_origen) DO UPDATE SET
                    tipo = EXCLUDED.tipo,
                    deposito_id = EXCLUDED.deposito_id,
                    creado_en = now(),
                    procesado_en = NULL,
                    error = NULL;";

            await context.Database.ExecuteSqlRawAsync(sql, empresaNormalizada, request.IdOrigen, tipo, (object?)request.DepositId ?? DBNull.Value);

            return Results.Ok(new { ok = true });
        })
        .WithName("MarcarTipoMovimiento")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/por-identificar", async (
            string empresa,
            DateOnly fechaDesde,
            DateOnly fechaHasta,
            string? search,
            AppDbContext context,
            CancellationToken cts,
            int offset = 0,
            int limit = 50
        ) =>
        {
            var empresaNormalizada = empresa?.Trim().ToUpperInvariant() ?? "";

            if (!EmpresasValidas.Contains(empresaNormalizada))
            {
                return Results.BadRequest($"Empresa invalida");
            }

            if (fechaHasta < fechaDesde)
            {
                return Results.BadRequest("fechaHasta no puede ser anterior a fechaDesde");
            }

            if ((fechaHasta.ToDateTime(TimeOnly.MinValue) -fechaDesde.ToDateTime(TimeOnly.MinValue)).TotalDays > 62)
            {
                return Results.BadRequest("El rango de fechas no puede superar 62 días");
            }

            if (offset < 0) offset = 0;
            if (limit <= 0) limit = 50;
            if (limit > 200) limit = 200;

            var desde = DateTime.SpecifyKind(fechaDesde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var hastaExclusivo = DateTime.SpecifyKind(fechaHasta.ToDateTime(TimeOnly.MinValue).AddDays(1), DateTimeKind.Utc);
            var searchPattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";

            const string sql = @"
                WITH t_cortado AS (
                    SELECT
                        m.id_origen, m.cuo, m.cuoa, m.banco, m.cta, m.fecha, m.descripcion, m.plaza,
                        m.nro_oper, m.cargo, m.abono, m.sd, m.comp, m.tipo,
                        m.tienda AS agencia, m.ruc, m.razon_social_cliente AS razon,
                        m.ubicacion, m.direccion, m.observacion,
                        SUM(COALESCE(rc.importe, 0)) AS reg,
                        CASE
                            WHEN COUNT(DISTINCT NULLIF(TRIM(rc.registro), '')) > 1 THEN 'VARIOS'
                            ELSE COALESCE(MAX(NULLIF(TRIM(rc.registro), '')), '')
                        END AS registro
                    FROM movimientos_bancarios m
                    LEFT JOIN registros_concar rc
                        ON rc.mcuo = m.cuo
                    AND rc.empresa = m.empresa
                    AND rc.estado <> 'ELIMINADO'
                    WHERE m.empresa = {0}
                        AND m.fecha >= {1}
                        AND m.fecha < {2}
                        AND m.abono > 0
                        AND m.tipo NOT IN ('LT', 'TJ', 'ITF')
                    GROUP BY
                        m.id_origen, m.cuo, m.cuoa, m.banco, m.cta, m.fecha, m.descripcion, m.plaza,
                        m.nro_oper, m.cargo, m.abono, m.sd, m.comp, m.tipo, m.tienda, m.ruc,
                        m.razon_social_cliente, m.ubicacion, m.direccion, m.observacion
                )
                SELECT
                    t.id_origen AS ""IdOrigen"",
                    t.cuo AS ""Cuo"",
                    t.cuoa AS ""Cuoa"",
                    t.banco AS ""Banco"",
                    t.cta AS ""Cta"",
                    t.fecha AS ""Fecha"",
                    t.descripcion AS ""Descripcion"",
                    t.plaza AS ""Plaza"",
                    t.nro_oper AS ""NroOper"",
                    t.cargo AS ""Cargo"",
                    t.abono AS ""Abono"",
                    t.sd AS ""Sd"",
                    t.comp AS ""Comp"",
                    t.tipo AS ""Tipo"",
                    t.agencia AS ""Agencia"",
                    t.ruc AS ""Ruc"",
                    t.razon AS ""Razon"",
                    t.ubicacion AS ""Ubicacion"",
                    t.direccion AS ""Direccion"",
                    t.observacion AS ""Observacion"",
                    t.reg AS ""Reg"",
                    t.registro AS ""Registro"",
                    ROUND(COALESCE(t.abono, 0)::numeric - COALESCE(t.cargo, 0)::numeric - t.reg, 2) AS ""Dif"",
                    s.""Nombre"" AS ""Sucursal"",
                    pv.""FullName"" AS ""Contacto"",
                    d.""TelefonoOrigen"" AS ""TelefonoContacto"",
                    pval.""FullName"" AS ""ValidadoPor""
                FROM t_cortado t
                LEFT JOIN depositos d
                    ON d.""Cuo"" = t.cuo
                LEFT JOIN sucursales s
                    ON s.""Id"" = d.""SucursalId""
                LEFT JOIN profiles pv
                    ON pv.""Id"" = d.""VendedorId""
                LEFT JOIN profiles pval
                    ON pval.""Id"" = d.""ValidadoPor""
                WHERE ABS(ROUND(COALESCE(t.abono, 0)::numeric - COALESCE(t.cargo, 0)::numeric - t.reg, 2)) > 0.00
                    AND ({3}::text IS NULL
                        OR t.cuo ILIKE {3} OR t.cuoa ILIKE {3} OR t.banco ILIKE {3} OR t.cta ILIKE {3}
                        OR t.nro_oper ILIKE {3} OR t.descripcion ILIKE {3} OR t.plaza ILIKE {3}
                        OR t.agencia ILIKE {3} OR t.razon ILIKE {3} OR t.ruc ILIKE {3} OR t.observacion ILIKE {3}
                        OR s.""Nombre"" ILIKE {3} OR pv.""FullName"" ILIKE {3})
                ORDER BY t.fecha DESC, t.banco ASC, t.cuo ASC
                LIMIT {5} OFFSET {4}";
            
            var movimientos = await context.Database
                .SqlQueryRaw<MovimientoPorIdentificarDto>(sql, empresaNormalizada, desde, hastaExclusivo, searchPattern!, offset, limit)
                .ToListAsync(cts);

            return Results.Ok(movimientos);
        })
        .WithName("GetMovimientosPorIdentificar")
        .Produces<List<MovimientoPorIdentificarDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}