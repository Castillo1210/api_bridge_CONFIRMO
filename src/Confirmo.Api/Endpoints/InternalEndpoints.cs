using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;
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

        group.MapPost("/webhooks/deposits/batch", async (
            [FromBody] BatchDepositsRequest request,
            HttpContext http,
            AppDbContext context,
            IStorageService storage,
            IPythonWorkerClient worker,
            ISignalRNotificationService notifications,
            ILogger<Program> logger
        ) =>
        {
            var userId = Guid.Parse(http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null) return Results.Unauthorized();

            var results = new List<object>();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in request.Items)
                {
                    byte[] imageBytes = Convert.FromBase64String(item.ImagenBase64);
                    var objectName = await storage.UploadVoucherAsync(user.EmpresaId, userId, imageBytes, "image/jpeg");

                    var deposit = new Deposito
                    {
                        Id = Guid.NewGuid(),
                        NumeroOperacion = item.NumeroOperacion,
                        Cliente = item.Cliente,
                        Monto = item.Monto,
                        Moneda = item.Moneda,
                        FechaDeposito = item.FechaDeposito,
                        BancoId = Guid.TryParse(item.BancoId, out var bId) ? bId : null,
                        Anexo = item.Anexo,
                        ReferenciaCliente = item.ReferenciaCliente,
                        RucCliente = item.RucCliente,
                        ImagenVoucher = objectName,
                        EmpresaId = user.EmpresaId,
                        SucursalId = user.SucursalId,
                        VendedorId = userId,
                        Estado = "recibido",
                        FechaRegistro = DateTimeOffset.UtcNow
                    };

                    context.Depositos.Add(deposit);
                    await context.SaveChangesAsync();
                    await worker.EnqueueProcessAsync(deposit.Id.ToString());
                    await notifications.NotifyDepositReceived(userId, deposit.Id);

                    results.Add(new { depositId = deposit.Id, estado = "recibido", numeroOperacion = item.NumeroOperacion });
                }

                await transaction.CommitAsync();
                return Results.Ok(new { items = results });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Error en batch deposits");
                return Results.BadRequest(new { error = "Error procesando lote", detail = ex.Message });
            }
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

public record DirectMessageRequest(Guid UserId, string Message, Guid? DepositId);
public record BatchDepositsRequest(List<DepositCreateRequest> Items);