using System.Security.Claims;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Confirmo.Api.Endpoints;

public static class VendedorChatEndpoints
{
    public static void MapVendedorChatEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/chat/vendedores/{vendedorId:guid}")
            .RequireAuthorization()
            .WithTags("Chat");

        group.MapGet("/", async (
            Guid vendedorId,
            HttpContext http,
            IChatService chat,
            [FromQuery] DateTimeOffset? before,
            [FromQuery] int limit = 50
        ) =>
        {
            if (!CanAccessVendedorChat(http, vendedorId)) return Results.Forbid();

            var history = await chat.GetVendedorHistoryAsync(vendedorId, before, limit);
            return Results.Ok(history);
        });

        group.MapPost("/", async (
            Guid vendedorId,
            HttpContext http,
            [FromBody] SendVendedorMessageRequest request,
            IChatService chat
        ) =>
        {
            var userId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = http.User.FindFirst(ClaimTypes.Role)?.Value;
            var isFinance = role is "admin" or "finanzas";

            var senderType = isFinance && userId != vendedorId ? "finance" : "vendedor";
            var type = string.IsNullOrEmpty(request.MessageType) ? "text" : request.MessageType;

            var message = await chat.AddVendedorMessageAsync(vendedorId, senderType, userId, request.Content ?? "", type);
        });
    }

    private static bool CanAccessVendedorChat(HttpContext http, Guid vendedorId)
    {
        var userId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role = http.User.FindFirst(ClaimTypes.Role)?.Value;
        var isFinance = role is "admin" or "finanzas";
        var isSelf = userId == vendedorId;

        return isFinance || isSelf;
    }
}