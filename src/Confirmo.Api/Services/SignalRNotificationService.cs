using Confirmo.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

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

    public Task NotifyDepositProcessing(Guid userId, Guid depositId, int progress)
        => SendAsync(userId, "DepositProcessing", new { depositId, progress, timestamp = DateTimeOffset.UtcNow });

    public Task NotifyDepositConfirmed(Guid userId, Guid depositId, object extractedData, string referenceNumber)
        => SendAsync(userId, "DepositConfirmed", new { depositId, extractedData, referenceNumber, timestamp = DateTimeOffset.UtcNow });

    public Task NotifyDepositRejected(Guid userId, Guid depositId, string reason)
        => SendAsync(userId, "DepositRejected", new { depositId, reason, timestamp = DateTimeOffset.UtcNow });

    public Task NotifyQualityRejected(Guid userId, Guid depositId, List<string> issues)
        => SendAsync(userId, "QualityRejected", new { depositId, issues, timestamp = DateTimeOffset.UtcNow });

    private Task SendAsync(Guid userId, string method, object payload)
    {
        _logger.LogDebug("SignalR {Method} para user {UserId}: {Payload}", method, userId, payload);
        return _hub.Clients.User(userId.ToString()).SendAsync(method, payload);
    }
}