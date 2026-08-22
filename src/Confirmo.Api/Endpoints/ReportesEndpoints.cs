using System.Security.Claims;
using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Endpoints;

public static class ReportesEndpoints
{
    private static readonly TimeSpan PeruOffset = TimeSpan.FromHours(-5);

    public static void MapReportesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/reportes")
            .RequireAuthorization()
            .WithTags("Reportes");

        // GET: tarjetas resumen
        group.MapGet("/summary", async ([FromQuery] string? period, HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || (user.Rol != "finanzas" && user.Rol != "admin"))
                return Results.Forbid();

            var (desde, hasta) = ResolverPeriodRange(period);

            var depositosPeriodo = await context.Depositos
                .AsNoTracking()
                .Where(d => d.FechaRegistro >= desde && d.FechaRegistro < hasta)
                .Select(d => new { d.Moneda, d.Estado, d.Monto })
                .ToListAsync();

            var summary = depositosPeriodo
                .GroupBy(d => d.Moneda)
                .Select(g => new ReporteMonedaSummary(
                    g.Key,
                    g.Where(d => d.Estado == DepositStates.Confirmado).Sum(d => d.Monto),
                    g.Count(),
                    g.Count(d => d.Estado == DepositStates.Confirmado)
                ))
                .ToList();

            return Results.Ok(new ReporteSummaryResponse(summary));
        });

        // GET: serie diaria de validados vs rechazados
        group.MapGet("/tendencia", async ([FromQuery] string? trendPeriod, HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || (user.Rol != "finanzas" && user.Rol != "admin"))
                return Results.Forbid();

            var dias = trendPeriod switch
            {
                "semana" => 7,
                "año" => 365,
                _ => 30
            };

            var desdeUtc = DateTimeOffset.UtcNow.AddDays(-dias);

            var depositos = await context.Depositos
                .AsNoTracking()
                .Where(d => d.FechaValidacion != null && d.FechaValidacion >= desdeUtc
                    && (d.Estado == DepositStates.Confirmado || d.Estado == DepositStates.Rechazado))
                .Select(d => new { d.Estado, d.FechaValidacion })
                .ToListAsync();

            var tendencia = depositos
                .GroupBy(d => d.FechaValidacion!.Value.ToOffset(PeruOffset).Date)
                .OrderBy(g => g.Key)
                .Select(g => new ReporteTendenciaDia(
                    g.Key.ToString("dd/MM"),
                    g.Count(d => d.Estado == DepositStates.Confirmado),
                    g.Count(d => d.Estado == DepositStates.Rechazado)
                ))
                .ToList();

            return Results.Ok(new ReporteTendenciaResponse(tendencia));
        });
    }

    private static (DateTimeOffset desde, DateTimeOffset hasta) ResolverPeriodRange(string? period)
    {
        var hoyPeru = DateTimeOffset.UtcNow.ToOffset(PeruOffset).Date;

        DateTime desdeFecha = period switch
        {
            "hoy" => hoyPeru,
            "semana" => hoyPeru.AddDays(-(((int)hoyPeru.DayOfWeek + 6) % 7)),
            _ => new DateTime(hoyPeru.Year, hoyPeru.Month, 1)
        };

        var desde = new DateTimeOffset(desdeFecha, PeruOffset).ToUniversalTime();
        var hasta = DateTimeOffset.UtcNow;

        return (desde, hasta);
    }

    private static Guid GetUserId(HttpContext http) => Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}