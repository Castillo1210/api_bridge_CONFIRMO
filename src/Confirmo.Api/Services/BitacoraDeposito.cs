using System.Text.Json;
using Confirmo.Api.Data;
using Confirmo.Api.Models.Entities;

namespace Confirmo.Api.Services;

public static class BitacoraDeposito
{
    private static readonly JsonSerializerOptions Camel = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static void Registrar(AppDbContext context, Guid depositoId, Guid? usuarioId, string evento, object? metadata = null)
    {
        context.DepositoEventos.Add(new DepositoEvento
        {
            DepositoId = depositoId,
            UsuarioId = usuarioId,
            Evento = evento,
            Origen = "servidor",
            Metadata = metadata == null ? null : JsonSerializer.Serialize(metadata, Camel),
            OcurridoEn = DateTimeOffset.UtcNow
        });
    }
}