namespace Confirmo.Api.Services;

public interface ISignalRNotificationService
{
    Task NotifyDepositReceived(Guid userId, Guid depositId);
    Task NotifyDepositProcessing(Guid userId, Guid depositId, int progress);
    Task NotifyDepositConfirmed(Guid userId, Guid depositId, object extractedData, string referenceNumber);
    Task NotifyDepositRejected(Guid userId, Guid depositId, string reason);
    Task NotifyQualityRejected(Guid userId, Guid depositId, List<string> issues);
}