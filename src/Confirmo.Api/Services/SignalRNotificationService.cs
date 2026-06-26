using Confirmo.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;

namespace Confirmo.Api.Services;

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<DepositHub> _hub;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(IHubContext<DepositHub> hub, ILogger<SignalRNotificationService> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public Task NotifyDepositReceived(Guid userId, Guid depositId)
        => SendAsync(userId, "DepositReceived", new { depositId, timestamp = DateTimeOffset.UtcNow });

    public Task NotifyDepositProcessing(Guid userId, Guid depositId, string message)
        => SendAsync(userId, "DepositProcessing", new { depositId, message, timestamp = DateTimeOffset.UtcNow });

    public Task NotifyDepositConfirmed(Guid userId, Guid depositId, object extractedData, string referenceNumber)
        => SendAsync(userId, "DepositConfirmed", new { depositId, extractedData, referenceNumber, timestamp = DateTimeOffset.UtcNow });

    public Task NotifyDepositRejected(Guid userId, Guid depositId, string reason)
        => SendAsync(userId, "DepositRejected", new { depositId, reason, timestamp = DateTimeOffset.UtcNow });

    public Task NotifyQualityRejected(Guid userId, Guid depositId, List<string> issues)
        => SendAsync(userId, "QualityRejected", new { depositId, issues, timestamp = DateTimeOffset.UtcNow });

    public Task SendChatMessage(Guid userId, ChatMessageResponse message)
        => _hub.Clients.User(userId.ToString()).SendAsync("ChatMessage", message);

    public Task SendDirectMessage(Guid userId, string message, Guid? depositId = null)
        => _hub.Clients.User(userId.ToString()).SendAsync("DirectMessage", new { message, depositId, timestamp = DateTimeOffset.UtcNow });

    private Task SendAsync(Guid userId, string method, object payload)
    {
        _logger.LogDebug("SignalR {Method} para user {UserId}: {Payload}", method, userId, payload);
        return _hub.Clients.User(userId.ToString()).SendAsync(method, payload);
    }

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
}