using System.Text.Json;
using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;
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
                var chat = scope.ServiceProvider.GetRequiredService<IChatService>();

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
                        await HandleResult(result, notifications, db, errorRepo, fcm, chat);
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

    private async Task HandleResult(WorkerResult result, ISignalRNotificationService notifications, AppDbContext db, IVoucherBusinessErrorRepository errorRepo, IFCMNotificationService fcm, IChatService chat)
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
        
        // CASO 1: IA falló (error_ia o error)
        if (result.Status is "error_ia" or "error")
        {
            _logger.LogInformation("Depósito {DepositId}: IA falló ({Status}), se mantiene procesado", result.DepositId, result.Status);
            return;
        }
        
        // CASO 2: Extracción exitosa -> aplicar reglas de negocio
        if (result.Status == "success")
        {
            var ruleResult = ApplyBusinessRules(deposit);

            if (ruleResult.IsRejected)
            {
                deposit.Estado = DepositStates.Rechazado;
                deposit.MotivoRechazo = ruleResult.RejectionReason;
                await db.SaveChangesAsync();
                
                await chat.AddSystemMessageAsync(deposit.Id, $"Depósito rechazado: {ruleResult.UserMessage}");

                await notifications.NotifyDepositRejectedWithDetails(deposit.VendedorId, deposit.Id, ruleResult.UserMessage!);

                await notifications.NotifyPanelDepositStatusChanged(deposit.Id, DepositStates.Rechazado, oldStatus);

                await SendFcmRejected(db, fcm, deposit.VendedorId, ruleResult.UserMessage!, _logger);
            }
            else
            {
                deposit.Estado = DepositStates.Procesado;
                deposit.Condicion = ruleResult.Condition;
                deposit.Riesgo = ruleResult.Risk;
                await db.SaveChangesAsync();

                await notifications.NotifyDepositProcessing(deposit.VendedorId, deposit.Id,
                    "Tu depósito fue procesado. Esperando confirmación");
            }
        }
        
        await db.SaveChangesAsync();
        _logger.LogInformation("Resultado procesado y notificado para depósito {DepositId}", result.DepositId);
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