namespace Confirmo.Api.Services;
using Confirmo.Api.Models.DTOs;

public interface IChatService
{
    Task AddMessageAsync(Guid depositId, string senderType, Guid? senderId, string content, string messageType, object? metadata = null);
    Task<ChatHistoryResponse> GetHistoryAsync(Guid depositId, Guid userId, DateTimeOffset? before = null, int limit = 50);
    Task SendDirectMessageAsync(Guid userId, string message, Guid? depositId = null);
    Task AddSystemMessageAsync(Guid depositId, string content, object? metadata = null);
}