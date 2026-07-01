using System.Globalization;
using System.Text.Json;
using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Google.Rpc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Confirmo.Api.Services;

public class WorkerResultConsumer : BackgroundService
{
    private readonly IRedisQueueService _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkerResultConsumer> _logger;
    private readonly IConfiguration _config;

    public WorkerResultConsumer(IRedisQueueService queue, IServiceScopeFactory scopeFactory, ILogger<WorkerResultConsumer> logger, IConfiguration config)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WorkerResultConsumer iniciado");

        // Asegurar Consumer Group
        await _queue.CreateConsumerGroupAsync("deposit:result:queue", "api-bridge");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var notifications = scope.ServiceProvider.GetRequiredService<ISignalRNotificationService>();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var errorRepo = scope.ServiceProvider.GetRequiredService<IVoucherBusinessErrorRepository>();
                var fcm = scope.ServiceProvider.GetRequiredService<IFCMNotificationService>();

                var entries = await _queue.ReadAsync("deposit:result:queue", "api-bridge", "consumer-1", 10, 5000);

                foreach (var entry in entries)
                {
                    try
                    {
                        var dataField = entry.Values.FirstOrDefault(v => v.Name == "data");
                        if (dataField.Value == RedisValue.Null)
                        {
                            _logger.LogWarning("Campo 'data' faltante en entry. Confirmando y descartando");
                            await _queue.AckAsync("deposit:result:queue", "api-bridge", entry.Id);
                            continue;
                        }

                        var data = dataField.Value.ToString();
                        var result = JsonSerializer.Deserialize<WorkerResult>(data);

                        if (result == null)
                        {
                            _logger.LogWarning("Deserialización de 'data' devolvió null para entry. Confirmado y descartando");
                            await _queue.AckAsync("deposit:result:queue", "api-bridge", entry.Id);
                            continue;
                        }
                        
                        // Procesar resultado
                        await HandleResult(result, notifications, db, errorRepo, fcm);

                        await _queue.AckAsync("deposit:result:queue", "api-bridge", entry.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando resultado del worker");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en WorkerResultConsumer loop");
                await Task.Delay(500, stoppingToken); // Backoff antes de reintentar
            }
        }
    }

    private async Task HandleResult(WorkerResult result, ISignalRNotificationService notifications, AppDbContext db, IVoucherBusinessErrorRepository errorRepo, IFCMNotificationService fcm)
    {
        var deposit = await db.Depositos
            .Include(d => d.Empresa)
            .Include(d => d.Sucursal)
            .Include(d => d.Banco)
            .FirstOrDefaultAsync(d => d.Id == Guid.Parse(result.DepositId));
        if (deposit == null)
        {
            _logger.LogWarning("Depósito no encontrado para resultado {DepositId}", result.DepositId);
            return;
        }

        _logger.LogInformation("Procesando resultado worker para depósito {DepositId}: {Status}", result.DepositId, result.Status);

        var oldStatus = deposit.Estado;

        deposit.Estado = MapWorkerStatusToDbStatus(result.Status);

        if (result.ErrorIds?.Count > 0)
        {
            deposit.ErrorIds = result.ErrorIds.Select(Guid.Parse).ToArray();
        }

        if (result.WarningIds?.Count > 0)
        {
            deposit.WarningIds = result.WarningIds.Select(Guid.Parse).ToArray();
        }

        await db.SaveChangesAsync();
        
        // Notificar al panel del admin
        await notifications.NotifyPanelDepositStatusChanged(deposit.Id, result.Status, oldStatus);

        switch (result.Status)
        {
            case "validado":
                var notification = new DepositConfirmedNotification(
                    DepositId: deposit.Id,
                    Estado: "validado",
                    ReferenceNumber: deposit.NumeroOperacionBanco ?? deposit.NumeroOperacion,
                    Empresa: deposit.Empresa?.Nombre ?? "",
                    Sucursal: deposit.Sucursal?.Nombre ?? "",
                    Banco: deposit.Banco?.Nombre ?? "",
                    Anexo: deposit.Anexo ?? "",
                    FechaDeposito: deposit.FechaDeposito ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    NumeroOperacion: deposit.NumeroOperacionBanco ?? deposit.NumeroOperacion,
                    Importe: deposit.Monto.ToString(CultureInfo.InvariantCulture),
                    Moneda: deposit.Moneda
                );
                await notifications.NotifyDepositConfirmed(deposit.VendedorId, deposit.Id, notification);
                
                // FCM Push
                try
                {
                    var vendedor = await db.Profiles.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == deposit.VendedorId);
                    if (vendedor?.FcmToken != null)
                    {
                        await fcm.SendDepositConfirmedAsync(vendedor.FcmToken, notification);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error enviando FCM para depósito {DepositId}", deposit.Id);
                }
                break;
            case "requiere_revision":
                if (result.WarningIds?.Count > 0)
                {
                    var warnings = await errorRepo.GetByIdsAsync(result.WarningIds);
                    await notifications.NotifyRequiresReview(deposit.VendedorId, deposit.Id, warnings);
                }
                break;
            case "rechazado":
                if (result.ErrorIds?.Count > 0)
                {
                    var errors = await errorRepo.GetByIdsAsync(result.ErrorIds);
                    await notifications.NotifyValidationErrors(deposit.VendedorId, deposit.Id, errors);
                    
                    // FCM push de rechazo
                    try
                    {
                        var vendedor = await db.Profiles.AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Id == deposit.VendedorId);
                        if (vendedor?.FcmToken != null)
                            await fcm.SendDepositRejectedAsync(vendedor.FcmToken, errors);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error enviando FCM rechazo para depósito {DepositId}", deposit.Id);
                    }
                }
                break;
            case "error_ia":
                await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id, "Recibimos tu depósito. Está en proceso de validación automática. Te notificaremos cuando termine.");
                break;
            default:
                _logger.LogWarning("Estado desconocido o error genérico: {Status}", result.Status);
                await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id, "Hubo un problema procesando tu depósito. Nuestro equipo lo revisará");
                break;
        }

        _logger.LogInformation("Resultado procesado y notificado para depósito {DepositId}", result.DepositId);
    }
    
    // Mapea el status del Worker Python al estado de BD
    private static string MapWorkerStatusToDbStatus(string workerStatus) => workerStatus switch
    {
        "error_id" => "recibido",
        "error" => "rechazado",
        _ => workerStatus
    };
}