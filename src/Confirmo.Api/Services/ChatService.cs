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
            var response = new ChatMessageResponse(msg.Id, msg.SenderType, msg.SenderId, msg.Content, msg.MessageType, msg.Metadata, msg.CreatedAt);
            await _signalR.SendChatMessage(deposit.VendedorId, response);
        }
    }
}