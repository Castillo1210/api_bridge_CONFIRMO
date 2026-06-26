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

        int backoffDelay = 500;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var notifications = scope.ServiceProvider.GetRequiredService<ISignalRNotificationService>();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var errorRepo = scope.ServiceProvider.GetRequiredService<IVoucherBusinessErrorRepository>();

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
                        await HandleResult(result, notifications, db, errorRepo);

                        await _queue.AckAsync("deposit:result:queue", "api-bridge", entry.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando resultado del worker");
                    }
                }
                backoffDelay = 500;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en WorkerResultConsumer loop");
                await Task.Delay(500, stoppingToken); // Backoff antes de reintentar
            }
        }
    }

    private async Task HandleResult(WorkerResult result, ISignalRNotificationService notifications, AppDbContext db, IVoucherBusinessErrorRepository errorRepo)
    {
        var deposit = await db.Depositos.FirstOrDefaultAsync(d => d.Id == Guid.Parse(result.DepositId));
        if (deposit == null)
        {
            _logger.LogWarning("Depósito no encontrado para resultado");
            return;
        }

        _logger.LogInformation("Procesando resultado worker");

        switch (result.Status)
        {
            case "validado":
                await notifications.NotifyDepositConfirmed(deposit.VendedorId, deposit.Id, deposit.DatosOcr!, deposit.NumeroOperacion ?? "");
                break;
            case "requiere_revision":
                if (result.WarningIds.Count > 0)
                {
                    var warnings = await errorRepo.GetByIdsAsync(result.WarningIds);
                    await notifications.NotifyRequiresReview(deposit.VendedorId, deposit.Id, warnings);
                }
                break;
            case "rechazado":
                if (result.ErrorIds.Count > 0)
                {
                    var errors = await errorRepo.GetByIdsAsync(result.ErrorIds);
                    await notifications.NotifyValidationErrors(deposit.VendedorId, deposit.Id, errors);
                }
                break;
            case "error_ia":
                await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id, "Recibimos tu depósito. Está en proceso de validación automática. Te notificaremos cuando termine.");
                break;
            case "error":
            default:
                _logger.LogWarning("Estado desconocido o error genérico");
                await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id, "Hubo un problema procesando tu depósito. Nuestro equipo lo revisará");
                break;
        }

        _logger.LogInformation("Resultado procesado y notificado");
    }
}