using Confirmo.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Confirmo.Api.Endpoints;

public static class ChatEndpoints
{
    public static void MapChatEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/deposits/{depositId:guid}/chat")
            .RequireAuthorization()
            .WithTags("Chat");

        group.MapGet("/", async (
            Guid depositId,
            HttpContext http,
            IChatService chat,
            [FromQuery] DateTimeOffset? before,
            [FromQuery] int limit = 50
        ) =>
        {
            var userId = Guid.Parse(http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var history = await chat.GetHistoryAsync(depositId, userId, before, limit);
            return Results.Ok(history);
        });

        group.MapPost("/", async (
            Guid depositId,
            HttpContext http,
            [FromBody] SendUserMessageRequest request,
            IChatService chat
        ) =>
        {
            var userId = Guid.Parse(http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            await chat.AddMessageAsync(depositId, "user", userId, request.Content, "text");
            return Results.Ok();
        });
    }
}

public record SendUserMessageRequest(string Content);
public record UploadChatImageRequest(string ImagenBase64);