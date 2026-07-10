using Confirmo.Api.Data;
using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Confirmo.Api.Models.DTOs;

namespace Confirmo.Api.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly ISignalRNotificationService _signalR;
    private readonly ILogger<ChatService> _logger;

    public ChatService(AppDbContext context, ISignalRNotificationService signalR, ILogger<ChatService> logger)
    {
        _context = context;
        _signalR = signalR;
        _logger = logger;
    }

    public async Task AddMessageAsync(Guid depositId, string senderType, Guid? senderId, string content, string messageType, object? metadata = null)
    {
        var msg = new DepositMessage
        {
            Id = Guid.NewGuid(),
            DepositId = depositId,
            SenderType = senderType,
            SenderId = senderId,
            Content = content,
            MessageType = messageType,
            Metadata = metadata,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.DepositMessages.Add(msg);
        await _context.SaveChangesAsync();

        var deposit = await _context.Depositos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == depositId);

        if (deposit != null)
        {
            var response = new ChatMessageResponse(msg.Id, msg.DepositId, msg.SenderType, msg.SenderId, msg.Content, msg.MessageType, msg.Metadata, msg.CreatedAt);
            await _signalR.SendChatMessage(deposit.VendedorId, response);
            await _signalR.NotifyPanelChatMessage(response, depositId);
        }
    }

    public async Task<ChatHistoryResponse> GetHistoryAsync(Guid depositId, Guid userId, DateTimeOffset? before = null, int limit = 50)
    {
        var deposit = await _context.Depositos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == depositId && d.VendedorId == userId);
        if (deposit == null) return new ChatHistoryResponse(new(), false);

        var query = _context.DepositMessages
            .AsNoTracking()
            .Where(m => m.DepositId == depositId);

        if (before.HasValue) query = query.Where(m => m.CreatedAt < before.Value);

        query = query.OrderByDescending(m => m.CreatedAt);

        var messages = await query.Take(limit + 1).ToListAsync();
        var hasMore = messages.Count > limit;
        var items = messages.Take(limit).Select(m => new ChatMessageResponse(m.Id, m.DepositId, m.SenderType, m.SenderId, m.Content, m.MessageType, m.Metadata, m.CreatedAt)).ToList();
        items.Reverse();

        return new ChatHistoryResponse(items, hasMore);
    }

    public async Task SendDirectMessageAsync(Guid userId, string message, Guid? depositId)
    {
        if (depositId.HasValue)
        {
            await AddMessageAsync(depositId.Value, "finance", null, message, "direct");
        }

        await _signalR.SendDirectMessage(userId, message, depositId);
    }

    public async Task AddSystemMessageAsync(Guid depositId, string content, object?  metadata = null)
    {
        await AddMessageAsync(depositId, "system", null, content, "status_change", metadata);
    }
}