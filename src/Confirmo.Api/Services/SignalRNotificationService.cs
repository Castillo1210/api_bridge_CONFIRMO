using Confirmo.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;

namespace Confirmo.Api.Services;

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<DepositHub> _hub;
    private readonly ILogger<SignalRNotificationService> _logger;

    private const string PANEL_GROUP = "panel";
    private const string FINANCE_GROUP = "finance";

    public SignalRNotificationService(IHubContext<DepositHub> hub, ILogger<SignalRNotificationService> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    // Notificaciones individuales
    public Task NotifyDepositReceived(Guid userId, Guid depositId)
        => SendToUser(userId, "DepositReceived", new { depositId, timestamp = DateTimeOffset.UtcNow });

    public Task NotifyDepositProcessing(Guid userId, Guid depositId, int progress)
        => SendToUser(userId, "DepositProcessing", new { depositId, progress, timestamp = DateTimeOffset.UtcNow });

    public Task NotifyDepositProcessing(Guid userId, Guid depositId, string message)
        => SendToUser(userId, "DepositProcessingUpdateStatusUpdate", new { status = "processing", message, timestamp = DateTimeOffset.UtcNow });

    public Task NotifyDepositConfirmed(Guid userId, Guid depositId, DepositConfirmedNotification notification)
    {
        _logger.LogInformation("Enviando notificación de confirmación a usuario");

        return _hub.Clients.User(userId.ToString()).SendAsync("DepositConfirmed", new 
        { depositId, notification, type = "deposit_confirmed", timestamp = DateTimeOffset.UtcNow });
    }

    public Task NotifyDepositRejected(Guid userId, Guid depositId, string reason)
        => SendToUser(userId, "DepositRejected", new { depositId, reason, timestamp = DateTimeOffset.UtcNow });

    public Task NotifyQualityRejected(Guid userId, Guid depositId, List<string> issues)
        => SendToUser(userId, "QualityRejected", new { depositId, issues, timestamp = DateTimeOffset.UtcNow });

    // Chat
    public Task SendChatMessage(Guid userId, ChatMessageResponse message)
        => _hub.Clients.User(userId.ToString()).SendAsync("ChatMessage", message);

    public Task SendDirectMessage(Guid userId, string message, Guid? depositId = null)
        => _hub.Clients.User(userId.ToString()).SendAsync("DirectMessage", new { message, depositId, timestamp = DateTimeOffset.UtcNow });

    // Validación de voucher
    public Task NotifyValidationErrors(Guid userId, Guid depositId, List<VoucherBusinessError> errors)
    {
        var payload = errors.Select(e => new
        {
            errorCode = e.ErrorCode,
            fieldName = e.FieldName,
            title = e.Title,
            message = e.Message,
            userAction = e.UserAction,
            severity = e.Severity
        });

        return _hub.Clients.User(userId.ToString()).SendAsync("ValidationErrors", new
        {
            depositId,
            errors = payload,
            timestamp = DateTimeOffset.UtcNow
        });
    }

    public Task NotifyRequiresReview(Guid userId, Guid depositId, List<VoucherBusinessError> warnings)
    {
        var payload = warnings.Select(w => new
        {
            warningCode = w.ErrorCode,
            fieldName = w.FieldName,
            title = w.Title,
            message = w.Message
        });

        return _hub.Clients.User(userId.ToString()).SendAsync("RequiresReview", new
        {
            depositId,
            warnings = payload,
            timestamp = DateTimeOffset.UtcNow
        });
    }

    // Panel Voucher
    public Task NotifyVoucherProcessing(Guid userId, Guid depositId, int progress, string stage, string? message = null)
    {
        return SendToUser(userId, "VoucherProcessing", new
        {
            depositId,
            progress,
            stage,
            message,
            timestamp = DateTimeOffset.UtcNow
        });
    }

    public Task NotifyVoucherOcrComplete(Guid userId, VoucherOcrResult ocrResult)
    {
        return SendToUser(userId, "VoucherOcrComplete", new
        {
            ocrResult.DepositId,
            ocrResult.NumeroOperacion,
            ocrResult.Banco,
            ocrResult.Monto,
            ocrResult.Moneda,
            ocrResult.Fecha,
            ocrResult.Confidence,
            timestamp = DateTimeOffset.UtcNow
        });
    }

    public Task NotifyVoucherOcrFailed(Guid userId, Guid depositId, string reason)
    {
        return SendToUser(userId, "VoucherOcrFailed", new
        {
            depositId,
            reason,
            timestamp = DateTimeOffset.UtcNow
        });
    }

    public Task NotifyVoucherError(Guid userId, VoucherErrorNotification error)
    {
        return SendToUser(userId, "VoucherError", new
        {
            error.DepositId,
            error.ErrorCode,
            error.Title,
            error.Message,
            error.UserAction,
            timestamp = DateTimeOffset.UtcNow
        });
    }
    
    // Panel Admin
    public async Task NotifyPanelNewDeposit(PanelDepositSummary deposit)
    {
        await _hub.Clients.Group(PANEL_GROUP).SendAsync("PanelNewDeposit", deposit);
        await _hub.Clients.Group(FINANCE_GROUP).SendAsync("PanelNewDeposit", deposit);
    }

    public async Task NotifyPanelDepositStatusChanged(Guid depositId, string newStatus, string oldStatus)
    {
        var payload = new { depositId, newStatus, oldStatus, timestamp = DateTimeOffset.UtcNow };
        await _hub.Clients.Group(PANEL_GROUP).SendAsync("PanelDepositStatusChanged", payload);
        await _hub.Clients.Group(FINANCE_GROUP).SendAsync("PanelDepositStatusChanged", payload);
    }

    public async Task NotifyPanelStatsUpdate(string group, PanelStatsUpdate stats)
    {
        var targetGroup = group switch
        {
            "finance" => FINANCE_GROUP,
            _ => PANEL_GROUP
        };
        await _hub.Clients.Group(targetGroup).SendAsync("PanelStatsUpdate", new
        {
            stats,
            timestamp = DateTimeOffset.UtcNow
        });
    }
    
    // Sistema
    public Task NotifyConnectionStatus(Guid userId, string status, string? message = null)
        => SendToUser(userId, "ConnectionStatus", new ConnectionStatusUpdate(status, DateTimeOffset.UtcNow, message));

    public Task NotifySystemAlert(Guid userId, SystemAlert alert)
        => SendToUser(userId, "SystemAlert", alert);

    public Task BroadcastSystemAlert(SystemAlert alert)
        => _hub.Clients.All.SendAsync("SystemAlert", alert);
    
    // Helpers
    private Task SendToUser(Guid userId, string method, object payload)
    {
        return _hub.Clients.User(userId.ToString()).SendAsync(method, payload);
    }
}