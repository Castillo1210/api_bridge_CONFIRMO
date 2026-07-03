using System.Globalization;
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
        
        // Webhook: sistema financiero externo
        group.MapPost("/webhooks/deposit-processed", async (
            [FromBody] ProcessedDepositCallback payload,
            AppDbContext context,
            ISignalRNotificationService notifications,
            IFCMNotificationService fcm,
            IChatService chat,
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

            await notifications.NotifyPanelDepositStatusChanged(deposit.Id, payload.Estado, oldStatus);

            switch (payload.Estado)
            {
                case "confirmado":
                    deposit.Estado = DepositStates.Confirmado;
                    deposit.FechaValidacion = DateTimeOffset.UtcNow;
                    deposit.Monto = payload.Monto;
                    deposit.Moneda = payload.Moneda;
                    deposit.FechaDeposito = payload.FechaDeposito;
                    deposit.BancoId = payload.BancoId;
                    deposit.EmpresaId = payload.EmpresaId;
                    deposit.ReferenciaCliente = payload.ReferenciaCliente;
                    deposit.NumeroOperacion = payload.NumeroOperacion ?? "";
                    deposit.DatosOcr = payload.DatosOcr;
                    
                    await context.SaveChangesAsync();

                    await chat.AddSystemMessageAsync(deposit.Id, "Depósito confirmado por el sistema financiero");
                    
                    var notif = new DepositConfirmedNotification(
                        DepositId: deposit.Id,
                        Estado: DepositStates.Confirmado,
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
                    await notifications.NotifyPanelDepositStatusChanged(deposit.Id, DepositStates.Confirmado, oldStatus);
                    await SendFcmToVendedor(context, fcm, deposit.VendedorId, notif, logger);
                    break;
                case "rechazado":
                    deposit.Estado = DepositStates.Rechazado;
                    deposit.MotivoRechazo = payload.MotivoRechazo;
                    deposit.DatosOcr = payload.DatosOcr;

                    await context.SaveChangesAsync();

                    await chat.AddSystemMessageAsync(deposit.Id, $"Depósito rechazado: {deposit.MotivoRechazo}");
                    
                    await notifications.NotifyDepositRejected(deposit.VendedorId, deposit.Id, payload.MotivoRechazo ?? "Tu depósito ha sido rechazado.");
                    await notifications.NotifyPanelDepositStatusChanged(deposit.Id, DepositStates.Rechazado, oldStatus);
                    break;
            }

            logger.LogInformation("Depósito {DepositId} actualizado a {Estado}", deposit.Id, payload.Estado);
            return Results.Ok();
        });
        
        // Python Worker notifica resultado (canal HTTP alternativo)
        group.MapPost("/webhooks/worker-result", async (
            [FromBody] WorkerResult payload,
            HttpContext http, AppDbContext context,
            ISignalRNotificationService notifications,
            IChatService chat,
            IFCMNotificationService fcm, ILogger<Program> logger
        ) =>
        {
            if (!http.Request.Headers.TryGetValue("X-Internal-Secret", out var secret)
                || secret != app.Configuration["InternalSecret"])
                return Results.Unauthorized();

            var deposit = await context.Depositos
                .Include(d => d.Empresa).Include(d => d.Sucursal).Include(d => d.Banco)
                .FirstOrDefaultAsync(d => d.Id == Guid.Parse(payload.DepositId));

            if (deposit == null) return Results.NotFound();

            // IA falló → mantener "procesado", notificar
            if (payload.Status is "error_ia" or "error")
            {
                await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id,
                    "Recibimos tu depósito. Está en proceso de validación. Te notificaremos.");
                return Results.Ok();
            }

            // Extracción exitosa → aplicar reglas de negocio
            var ruleResult = ApplyBusinessRules(deposit);

            if (ruleResult.IsRejected)
            {
                var oldStatus = deposit.Estado;
                deposit.Estado = DepositStates.Rechazado;
                deposit.MotivoRechazo = ruleResult.RejectionReason;
                await context.SaveChangesAsync();

                await chat.AddSystemMessageAsync(deposit.Id, $"Depósito rechazado: {ruleResult.RejectionReason}");
                await notifications.NotifyDepositRejectedWithDetails(
                    deposit.VendedorId, deposit.Id, ruleResult.UserMessage!);
                await notifications.NotifyPanelDepositStatusChanged(deposit.Id, DepositStates.Rechazado, oldStatus);
                await SendFcmRejected(context, fcm, deposit.VendedorId, ruleResult.UserMessage!, logger);
            }
            else if (ruleResult.UserMessage != null)
            {
                await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id, ruleResult.UserMessage);
            }
            else
            {
                await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id,
                    "Tu depósito fue procesado. Esperando confirmación.");
            }

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
    
    private static BusinessRuleResult ApplyBusinessRules(Deposito deposit)
    {
        // Determinar qué campos faltan (no extraídas por la IA)
        var missingFields = new List<string>();
        
        if (deposit.Monto <= 0)
            missingFields.Add("monto");
        if (string.IsNullOrWhiteSpace(deposit.Moneda))
            missingFields.Add("moneda");
        if (!deposit.FechaDeposito.HasValue)
            missingFields.Add("fecha");
        if (string.IsNullOrWhiteSpace(deposit.NumeroOperacion))
            missingFields.Add("numero_operacion");
        
        // Inicializar variables por defecto
        var risk = false;
        var condition = "actual";
        var isRejected = false;

        string? rejectionReason = null;
        string? userMessage = null;
        
        // Regla 1: 2+ campos faltantes -> poner riesgo
        if (missingFields.Count >= 2)
        {
            risk = true;
        }
        
        // Regla 2: Validación de fecha -> poner condición
        if (deposit.FechaDeposito.HasValue)
        {
            var fecha = deposit.FechaDeposito.Value;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (fecha > today)
            {
                isRejected = true;
                rejectionReason = $"La fecha del voucher ({fecha:yyyy-MM-dd}) es futura.";
                userMessage =
                    $"La fecha del voucher ({fecha:yyyy-MM-dd}) es futura. Verificá y regularizá el depósito.";
            }

            if (fecha < today)
            {
                condition = "antiguo";
            }
        }

        return new BusinessRuleResult(IsRejected: isRejected, Risk: risk, Condition: condition, RejectionReason: rejectionReason, UserMessage: userMessage);
    }
    
    private static async Task SendFcmRejected(AppDbContext context, IFCMNotificationService fcm, Guid vendedorId,
        string message, ILogger logger)
    {
        try
        {
            var vendedor = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == vendedorId);
            if (vendedor?.FcmToken != null)
            {
                await fcm.SendNotificationAsync(vendedor.FcmToken, "Depósito Rechazado", message,
                    new Dictionary<string, string> { ["type"] = "deposit_rejected" });
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error enviando FCM de rechazo");
        }
    }
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
    Guid? EmpresaId,
    string? ReferenciaCliente,
    string? ReferenceNumber,
    List<string>? QualityIssues
);

