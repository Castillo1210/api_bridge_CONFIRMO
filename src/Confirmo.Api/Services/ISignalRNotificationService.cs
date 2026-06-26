namespace Confirmo.Api.Services;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;

public interface ISignalRNotificationService
{
    Task NotifyDepositReceived(Guid userId, Guid depositId);
    Task NotifyDepositProcessing(Guid userId, Guid depositId, string message);
    Task NotifyDepositConfirmed(Guid userId, Guid depositId, object extractedData, string referenceNumber);
    Task NotifyDepositRejected(Guid userId, Guid depositId, string reason);
    Task NotifyQualityRejected(Guid userId, Guid depositId, List<string> issues);
    Task SendChatMessage(Guid userId, ChatMessageResponse message);
    Task SendDirectMessage(Guid userId, string message, Guid? depositId = null);
    Task NotifyValidationErrors(Guid userId, Guid depositId, List<VoucherBusinessError> errors);
    Task NotifyRequiresReview(Guid userId, Guid depositId, List<VoucherBusinessError> warnings);
}