namespace Confirmo.Api.Services;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;

public interface ISignalRNotificationService
{
    // Notificaciones de depósitos individuales (vendedor)
    Task NotifyDepositReceived(Guid userId, Guid depositId);
    Task NotifyDepositProcessing(Guid userId, Guid depositId, string message);
    Task NotifyDepositConfirmed(Guid userId, Guid depositId, DepositConfirmedNotification notification);
    Task NotifyDepositRejected(Guid userId, Guid depositId, string reason);
    Task NotifyDepositRejectedWithDetails(Guid userId, Guid depositId, string reason);
    
    Task NotifyQualityRejected(Guid userId, Guid depositId, List<string> issues);
    
    // Chat
    Task SendChatMessage(Guid userId, ChatMessageResponse message);
    Task SendDirectMessage(Guid userId, string message, Guid? depositId = null);
    Task NotifyVendedorChatMessage(VendedorMessageResponse message);

    Task NotifyValidationErrors(Guid userId, Guid depositId, List<VoucherBusinessError> errors);
    Task NotifyRequiresReview(Guid userId, Guid depositId, List<VoucherBusinessError> warnings);
    

    // Panel Voucher
    Task NotifyVoucherProcessing(Guid userId, Guid depositId, int progress, string stage, string? message = null);
    Task NotifyVoucherOcrComplete(Guid userId, VoucherOcrResult ocrResult);
    Task NotifyVoucherOcrFailed(Guid userId, Guid depositId, string reason);
    Task NotifyVoucherError(Guid userId, VoucherErrorNotification error);

    // Panel Admin / Finanzas
    Task NotifyPanelNewDeposit(PanelDepositSummary deposit);
    Task NotifyPanelDepositStatusChanged(Guid depositId, string newStatus, string oldStatus);
    Task NotifyPanelStatsUpdate(string group, PanelStatsUpdate stats);
    Task NotifyPanelDepositLocked(Guid depositId, Guid validateBy, string? validateByName);
    Task NotifyPanelDepositUnlocked(Guid depositId);

    // Sistema
    Task NotifyConnectionStatus(Guid userId, string status, string? message = null);
    Task NotifySystemAlert(Guid userId, SystemAlert alert);
    Task BroadcastSystemAlert(SystemAlert alert);
    Task NotifyPanelChatMessage(ChatMessageResponse message, Guid depositId);
}