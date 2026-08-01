namespace Confirmo.Api.Services;

public record ZavuSendResult(bool Success, string? MessageId, string? Error);

public interface IZavuClient
{
    Task<ZavuSendResult> SendAsync(
        string to, string text, string channel,
        string? idempotencyKey = null, string? subject = null,
        CancellationToken cts = default
    );
}