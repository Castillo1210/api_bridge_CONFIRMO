using System.Globalization;
using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;
using Confirmo.Api.Services;
using Google.Api;
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
            IFCMNotificationService fcm,
            ILogger<Program> logger
        ) =>
        {
            var deposit = await context.Depositos
                .Include(d => d.Empresa)
                .Include(d => d.Sucursal)
                .Include(d => d.Banco)
                .FirstOrDefaultAsync(d => d.Id == payload.DepositId);
            
            if (deposit == null) return Results.NotFound();

            var oldStatus = deposit.Estado;
            deposit.Estado = payload.Estado;
            deposit.FechaValidacion = payload.Estado == "confirmado" ? DateTimeOffset.UtcNow : null;
            deposit.MotivoRechazo = payload.MotivoRechazo;
            deposit.DatosOcr = payload.DatosOcr;
            
            if (payload.Estado == "confirmado")
            {
                deposit.Monto = payload.Monto;
                deposit.Moneda = payload.Moneda;
                deposit.FechaDeposito = payload.FechaDeposito;
                deposit.BancoId = payload.BancoId;
                deposit.ReferenciaCliente = payload.ReferenciaCliente;
                deposit.NumeroOperacion = payload.NumeroOperacion ?? "";
            }

            await context.SaveChangesAsync();

            await notifications.NotifyPanelDepositStatusChanged(deposit.Id, payload.Estado, oldStatus);

            switch (payload.Estado)
            {
                case "confirmado":
                    var notif = new DepositConfirmedNotification(
                        DepositId: deposit.Id,
                        Estado: "confirmado",
                        ReferenceNumber: payload.ReferenceNumber ?? deposit.NumeroOperacionBanco ?? deposit.NumeroOperacion ?? "",
                        Empresa: deposit.Empresa?.Nombre ?? "",
                        Sucursal: deposit.Sucursal?.Nombre ?? "",
                        Banco: deposit.Banco?.Nombre ?? "",
                        Anexo: deposit.Anexo ?? "",
                        FechaDeposito: deposit.FechaDeposito ?? DateOnly.FromDateTime(DateTime.UtcNow),
                        NumeroOperacion: deposit.NumeroOperacionBanco ?? deposit.NumeroOperacion ?? "",
                        Importe: deposit.Monto.ToString(CultureInfo.InvariantCulture),
                        Moneda: deposit.Moneda
                        );
                    await notifications.NotifyDepositConfirmed(deposit.VendedorId, deposit.Id, notif);
                    await SendFcmToVendedor(context, fcm, deposit.VendedorId, notif, logger);
                    break;
                /*case "requiere_revision":
                    await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id,
                        "Tu depósito requiere revisión manual. Te notificaremos cuando esté listo.");
                    break;*/
                case "rechazado":
                    await notifications.NotifyDepositRejected(deposit.VendedorId, deposit.Id, payload.MotivoRechazo ?? "Tu depósito ha sido rechazado.");
                    break;
                case "calidad_rechazado":
                    await notifications.NotifyQualityRejected(deposit.VendedorId, deposit.Id, payload.QualityIssues ?? new List<string>());
                    break;
                /*case "error_ia":
                    await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id,
                        "Recibimos tu depósito. Está en proceso de validación automática. Te notificaremos cuando termine.");
                    break;*/
            }

            logger.LogInformation("Depósito {DepositId} actualizado a {Estado}", deposit.Id, payload.Estado);
            return Results.Ok();
        });
        
        // WebHook: batch de depósitos
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
                        Cliente = item.Cliente,
                        BancoId = Guid.TryParse(item.BancoId, out var bId) ? bId : null,
                        ImagenVoucher = objectName,
                        EmpresaId = Guid.TryParse(item.BancoId, out var eId) ? eId : null,
                        SucursalId = user.SucursalId,
                        VendedorId = userId,
                        Estado = "recibido",
                        FechaRegistro = DateTimeOffset.UtcNow
                    };

                    context.Depositos.Add(deposit);
                    await context.SaveChangesAsync();
                    await worker.EnqueueProcessAsync(deposit.Id.ToString());
                    await notifications.NotifyDepositReceived(userId, deposit.Id);

                    results.Add(new { depositId = deposit.Id, estado = "recibido" });
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
        
        // Webhook: resultado del Worker Python
        group.MapPost("/webhooks/worker-result", async(
            [FromBody] WorkerResult payload,
            HttpContext http,
            AppDbContext context,
            ISignalRNotificationService notifications,
            IVoucherBusinessErrorRepository errorRepo,
            IFCMNotificationService fcm,
            ILogger<Program> logger
        ) =>
        {
            if (!http.Request.Headers.TryGetValue("X-Internal-Secret", out var secret) || secret != app.Configuration["InternalSecret"])
            {
                return Results.Unauthorized();
            }

            var deposit = await context.Depositos
                .Include(d => d.Empresa)
                .Include(d => d.Sucursal)
                .Include(d => d.Banco)
                .FirstOrDefaultAsync(d => d.Id == Guid.Parse(payload.DepositId));
            if (deposit == null) return Results.NotFound();

            // Actualizar BD con resultado
            var oldStatus = deposit.Estado;
            deposit.Estado = MapWorkerStatusToDbStatus(payload.Status);

            if (payload.ErrorIds?.Count > 0)
            {
                deposit.ErrorIds = payload.ErrorIds.Select(Guid.Parse).ToArray();
            }
            if (payload.WarningIds?.Count > 0)
            {
                deposit.WarningIds = payload.WarningIds.Select(Guid.Parse).ToArray();
            }

            await context.SaveChangesAsync();
            await notifications.NotifyPanelDepositStatusChanged(deposit.Id, payload.Status, oldStatus);

            switch (payload.Status)
            {
                case "validado":
                    var notif = new DepositConfirmedNotification(
                        DepositId: deposit.Id,
                        Estado: "validado",
                        ReferenceNumber: deposit.NumeroOperacionBanco ?? deposit.NumeroOperacion,
                        Empresa: deposit.Empresa?.Nombre ?? "",
                        Sucursal: deposit.Sucursal?.Nombre ?? "",
                        Banco: deposit.Banco?.Nombre ?? "",
                        Anexo: deposit.Anexo ?? "",
                        FechaDeposito: deposit.FechaDeposito ?? DateOnly.FromDateTime(DateTime.UtcNow),
                        NumeroOperacion: deposit.NumeroOperacionBanco ?? deposit.NumeroOperacion ?? "",
                        Importe: deposit.Monto.ToString(CultureInfo.InvariantCulture),
                        Moneda: deposit.Moneda
                    );
                    await notifications.NotifyDepositConfirmed(deposit.VendedorId, deposit.Id, notif);
                    await SendFcmToVendedor(context, fcm, deposit.VendedorId, notif, logger);
                    break;
                case "requiere_revision":
                    if (payload.WarningIds?.Count > 0)
                    {
                        var warnings = await errorRepo.GetByIdsAsync(payload.WarningIds);
                        await notifications.NotifyRequiresReview(deposit.VendedorId, deposit.Id, warnings);
                    }
                    break;
                case "rechazado":
                    if (payload.ErrorIds?.Count > 0)
                    {
                        var errors = await errorRepo.GetByIdsAsync(payload.ErrorIds);
                        await notifications.NotifyValidationErrors(deposit.VendedorId, deposit.Id, errors);
                    }
                    break;
                case "error_ia":
                    await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id,
                        "Recibimos tu depósito. Está en proceso de validación automática. Te notificaremos cuando termine.");
                    break;
                default:
                    await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id,
                        "Hubo un problema procesando tu depósito. Nuestro equipo lo revisará.");
                    break;
            }
            
            logger.LogInformation("Worker-result procesado: depósito {DepositId} -> {Status}", deposit.Id, payload.Status);
            return Results.Ok();
        });
    }

    private static async Task SendFcmToVendedor(AppDbContext context, IFCMNotificationService fcm, Guid vendedorId,
        DepositConfirmedNotification notification, ILogger logger)
    {
        try
        {
            var vendedor = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == vendedorId);

            if (vendedor?.FcmToken != null)
            {
                await fcm.SendDepositConfirmedAsync(vendedor.FcmToken, notification);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error enviando FCM para depósito {DepositId}", notification.DepositId);
        }
    }
    
    // Mapea el status del Worker Python al estado de BD
    private static string MapWorkerStatusToDbStatus(string workerStatus) => workerStatus switch
    {
        "error_id" => "recibido",
        "error" => "rechazado",
        _ => workerStatus
    };
}

public record ProcessedDepositCallback(
    Guid DepositId,
    string Estado, // confirmado | rechazado | calidad_rechazado
    object? DatosOcr,
    string? MotivoRechazo,
    string? NumeroOperacion,
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