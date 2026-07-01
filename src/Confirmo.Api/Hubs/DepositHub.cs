using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Confirmo.Api.Hubs;

[Authorize]
public class DepositHub : Hub
{
    private readonly ILogger<DepositHub> _logger;
    
    // Track de conexiones activas por userId (para detectar duplicados/múltiples dispositivos)
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();

    public DepositHub(ILogger<DepositHub> logger)
    {
        _logger = logger;
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Conexión rechazada: sin UserIdentifier");
            Context.Abort();
            return;
        }

        _userConnections.AddOrUpdate(
            userId,
            _ => new HashSet<string> {Context.ConnectionId},
            (_, set) => { 
                set.Add(Context.ConnectionId);
                return set; 
            });

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(Context.ConnectionId);
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    // Métodos Cliente -> Servidor
    public async Task JoinDepositGroup(string depositId)
    {
        try
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            var groupName = $"deposit:{depositId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Caller.SendAsync("GroupJoined", new
            {
                group = groupName,
                depositId,
                timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", new
            {
                method = "JoinDepositGroup",
                message = "Error al unirse al grupo del depósito",
                timestamp = DateTimeOffset.UtcNow
            }); 
        }
    }
    
    // Salir del grupo de un depósito
    public async Task LeaveDepositGroup(string depositId)
    {
        try
        {
            var groupName = $"deposit:{depositId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Caller.SendAsync("GroupLeft", new
            {
                group = groupName,
                depositId,
                timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en LeaveDepositGroup para depósito");
        }
    }
    
    public async Task JoinPanelGroup()
    {
        try
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            // Verificar rol via claims
            var roleClaim = Context.User?.FindFirst("rol")?.Value
                            ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (roleClaim is not ("finanzas" or "admin"))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    method = "JoinPanelGroup",
                    message = "Acceso denegado: se requiere rol finanzas o admin",
                    timestamp = DateTimeOffset.UtcNow
                });
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, "panel");
            if (roleClaim == "finanzas")
                await Groups.AddToGroupAsync(Context.ConnectionId, "finance");

            _logger.LogInformation("User {UserId} (rol={Role}) se unió al panel", userId, roleClaim);

            await Clients.Caller.SendAsync("PanelJoined", new
            {
                role = roleClaim,
                timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en JoinPanelGroup");
        }
    }

    /// <summary>Salir del panel de administración</summary>
    public async Task LeavePanelGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "panel");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "finance");
    }
    
    // Indicador de "escribiendo..." en chat
    public async Task SendTyping(string depositId, bool isTyping)
    {
        try
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            await Clients.Group($"deposit:{depositId}").SendAsync("UserTyping", new
            {
                userId,
                depositId,
                isTyping,
                timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en SendTyping");
        }
    }
    
    // Marcar mensajes como leídos
    public async Task MarkMessagesRead(string depositId, List<string> messageIds)
    {
        try
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            await Clients.Group($"deposit:{depositId}").SendAsync("MessagesRead", new
            {
                userId,
                depositId,
                messageIds,
                timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en MarkMessagesRead");
        }
    }
    
    // Ping desde el cliente
    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", new
        {
            serverTime = DateTimeOffset.UtcNow,
            connectionId = Context.ConnectionId
        });
    }
    
    // Registrar FCM token del dispositivo
    public async Task RegisterFcmToken(string fcmToken)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("Error", new
            {
                method = "RegisterFcmToken",
                message = "Usuario no autenticado",
                timestamp = DateTimeOffset.UtcNow
            });
            return;
        }
        
        _logger.LogInformation("FCM token registrado para el usuario");
        await Clients.Caller.SendAsync("FcmTokenRegistered", new
        {
            success = true,
            timestamp = DateTimeOffset.UtcNow
        });
    }
}