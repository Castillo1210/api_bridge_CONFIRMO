using Confirmo.Api.Data;
using Confirmo.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Endpoints;

public static class InternalEndpoints
{
    public static void MapInternalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/internal").RequireHost("localhost", "127.0.0.1", "api-bridge");

        group.MapPost("/webhooks/deposit-processed", async (
            [FromBody] ProcessedDepositCallback payload,
            AppDbContext context,
            ISignalRNotificationService notifications,
            ILogger<Program> logger
        ) =>
        {
            var deposit = await context.Depositos.FirstOrDefaultAsync(d => d.Id == payload.DepositId);
            if (deposit == null) return Results.NotFound();

            deposit.Estado = payload.Estado;
            deposit.FechaValidacion = payload.Estado == "confirmado" ? DateTimeOffset.UtcNow : null;
            deposit.MotivoRechazo = payload.MotivoRechazo;
            deposit.DatosOcr = payload.DatosOcr;
            deposit.NumeroOperacionBanco = payload.NumeroOperacionBanco;

            if (payload.Estado == "confirmado")
            {
                deposit.Monto = payload.Monto;
                deposit.Moneda = payload.Moneda;
                deposit.FechaDeposito = payload.FechaDeposito;
                deposit.BancoId = payload.BancoId;
                deposit.ReferenciaCliente = payload.ReferenciaCliente;
            }

            await context.SaveChangesAsync();

            switch (payload.Estado)
            {
                case "confirmado":
                    await notifications.NotifyDepositConfirmed(deposit.VendedorId, deposit.Id, payload.DatosOcr!, payload.ReferenceNumber!);
                    break;
                case "rechazado":
                    await notifications.NotifyDepositRejected(deposit.VendedorId, deposit.Id, payload.MotivoRechazo!);
                    break;
                case "calidad_rechazado":
                    await notifications.NotifyQualityRejected(deposit.VendedorId, deposit.Id, payload.QualityIssues!);
                    break;
            }

            logger.LogInformation("Depósito {DepositId} actualizado a {Estado}", deposit.Id, payload.Estado);
            return Results.Ok();
        });
    }
}

public record ProcessedDepositCallback(
    Guid DepositId,
    string Estado, // confirmado | rechazado | calidad_rechazado
    object? DatosOcr,
    string? MotivoRechazo,
    string? NumeroOperacionBanco,
    decimal Monto,
    string Moneda,
    DateOnly? FechaDeposito,
    Guid? BancoId,
    string? ReferenciaCliente,
    string? ReferenceNumber,
    List<string>? QualityIssues
);