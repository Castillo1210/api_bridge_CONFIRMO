using System.Security.Claims;
using System.Text.Json;
using Confirmo.Api.Data;
using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Endpoints;

public record DepositEventInput(Guid DepositId, string Evento, Guid? SesionId, DateTimeOffset? OcurridoEn, JsonElement? Metadata);
public record DepositEventBatchRequest(List<DepositEventInput>? Events);

public static class DepositEventEndpoints
{
    private static readonly HashSet<string> EventosWeb = new() { "detalle_abierto", "detalle_cerrado", "accion" };
    private const int MaxLote = 100;
    private const int MaxMetadata = 4000;

    public static void MapDepositEventEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/deposit-events", async (DepositEventBatchRequest request, HttpContext http, AppDbContext context) =>
        {
            var rol = http.User.FindFirst(ClaimTypes.Role)?.Value;
            if (rol is not ("finanzas" or "admin")) return Results.Forbid();
            if (!Guid.TryParse(http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Results.Unauthorized();
            
            var events = request.Events ?? new();
            if (events.Count == 0) return Results.Ok(new { accepted = 0 });
            if (events.Count > MaxLote) return Results.BadRequest(new { error = $"Máximo {MaxLote} eventos por lote." });

            // Descarta ids inexistentes en vez de fallar todo el lote por la FK
            var ids = events.Select(e => e.DepositId).Distinct().ToList();
            var existentes = (await context.Depositos.AsNoTracking()
                .Where(d => ids.Contains(d.Id)).Select(d => d.Id).ToListAsync()).ToHashSet();

            var ahora = DateTimeOffset.UtcNow;
            var aceptados = 0;
            foreach (var e in events)
            {
                if (!existentes.Contains(e.DepositId) || !EventosWeb.Contains(e.Evento ?? "")) continue;

                // Hora del navegador solo si es razonable (relojes desfasados / colas viejas)
                var ocurrido = e.OcurridoEn ?? ahora;
                if (ocurrido > ahora.AddMinutes(2) || ocurrido < ahora.AddHours(-48)) ocurrido = ahora;

                string? metadata = null;
                if (e.Metadata is { ValueKind: JsonValueKind.Object } m)
                {
                    var raw = m.GetRawText();
                    metadata = raw.Length <= MaxMetadata ? raw : null;
                }

                context.DepositoEventos.Add(new DepositoEvento
                {
                    DepositoId = e.DepositId,
                    UsuarioId = userId,
                    Evento = e.Evento!,
                    Origen = "web",
                    SesionId = e.SesionId,
                    Metadata = metadata,
                    OcurridoEn = ocurrido
                });
                aceptados++;
            }

            if (aceptados > 0) await context.SaveChangesAsync();
            return Results.Ok(new { accepted = aceptados });
        })
        .RequireAuthorization()
        .WithTags("Deposits")
        .WithSummary("Bitácora: eventos del panel (abrir/cerrar detalle, acciones)");
    }
}