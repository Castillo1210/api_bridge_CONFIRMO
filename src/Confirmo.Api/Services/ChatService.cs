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

    public async Task<VendedorMessageResponse> AddVendedorMessageAsync(Guid vendedorId, string senderType, Guid? senderId, string content, string messageType = "text")
    {
        var msg = new VendedorMessage
        {
            Id = Guid.NewGuid(),
            VendedorId = vendedorId,
            SenderType = senderType,
            SenderId = senderId,
            Content = content,
            MessageType = messageType,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.VendedorMessages.Add(msg);
        await _context.SaveChangesAsync();

        var response = new VendedorMessageResponse(msg.Id, msg.VendedorId, msg.SenderType, msg.SenderId, msg.Content, msg.MessageType, msg.CreatedAt);

        await _signalR.NotifyVendedorChatMessage(response);

        return response;
    }

    public async Task<VendedorChatHistoryResponse> GetVendedorHistoryAsync(Guid vendedorId, DateTimeOffset? before = null, int limit = 50)
    {
        var query = _context.VendedorMessages
            .AsNoTracking()
            .Where(m => m.VendedorId == vendedorId);

        if (before.HasValue) query = query.Where(m => m.CreatedAt < before.Value);

        query = query.OrderByDescending(m => m.CreatedAt);

        var messages = await query.Take(limit + 1).ToListAsync();
        var hasMore = messages.Count > limit;
        var items = messages.Take(limit)
            .Select(m => new VendedorMessageResponse(m.Id, m.VendedorId, m.SenderType, m.SenderId, m.Content, m.MessageType, m.CreatedAt))
            .ToList();
        
        items.Reverse();

        return new VendedorChatHistoryResponse(items, hasMore);
    }

    public async Task<string> RenderPlantillaAsync(string codigo, Dictionary<string, string?> valores)
    {
        var plantilla = await _context.PlantillasMensajesSistema.AsNoTracking().FirstOrDefaultAsync(p => p.Codigo == codigo && p.Activo);

        var contenido = plantilla?.Contenido ?? $"[Falta configurar la plantilla '{codigo}']";

        foreach (var (token, valor) in valores)
        {
            contenido = contenido.Replace("{{" + token + "}}", string.IsNullOrWhiteSpace(valor) ? "N/A" : valor);
        }

        contenido = System.Text.RegularExpressions.Regex.Replace(contenido, @"\{\{\s*\w+\s*\}\}", "N/A");

        return contenido;
    }

    public static Dictionary<string, string?> BuildDepositPlaceholders(Models.Entities.Deposito deposito, string? observaciones = null)
    {
        return new Dictionary<string, string?>
        {
            ["empresa"] = deposito.Empresa?.Nombre,
            ["sucursal"] = deposito.Sucursal?.Nombre,
            ["banco"] = deposito.Banco?.Nombre,
            ["anexo"] = deposito.Anexo,
            ["fecha_deposito"] = deposito.FechaDeposito?.ToString("dd/MM/yyyy"),
            ["operacion"] = deposito.NumeroOperacionBanco ?? deposito.NumeroOperacion,
            ["importe"] = $"{deposito.Moneda} {deposito.Monto:0.00}",
            ["cliente"] = deposito.Cliente,
            ["observaciones"] = observaciones ?? deposito.Observaciones,
        };
    }
}