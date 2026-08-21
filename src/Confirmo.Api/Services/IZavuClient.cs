namespace Confirmo.Api.Services;

public record ZavuSendResult(bool Success, string? MessageId, string? Error);

public interface IZavuClient
{
    Task<ZavuSendResult> SendAsync(
        string to, string text, string channel,
        string? idempotencyKey = null, string? subject = null,
        CancellationToken cts = default
    );

    Task<ZavuSendResult> SendTemplateAsync(
        string to, string templateId, Dictionary<string, string> templateVariables,
        string? idempotencyKey = null,
        CancellationToken cts = default
    );
}