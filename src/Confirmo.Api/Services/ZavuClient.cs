using System.Text.Json;

namespace Confirmo.Api.Services;

public class ZavuClient : IZavuClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ZavuClient> _logger;

    public ZavuClient(HttpClient http, ILogger<ZavuClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ZavuSendResult> SendAsync(string to, string text, string channel, string? idempotencyKey = null, string? subject = null, CancellationToken cts = default)
    {
        try
        {
            var payload = new Dictionary<string, object?>
            {
                ["to"] = to,
                ["text"] = text,
                ["channel"] = channel,
            };

            if (idempotencyKey != null)
            {
                payload["idempotencyKey"] = idempotencyKey;
            }

            if (subject != null) payload["subject"] = subject;

            var response = await _http.PostAsJsonAsync("/v1/messages", payload, cts);
            var body = await response.Content.ReadAsStringAsync(cts);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Zavu devolvió {Status} para {Channel}: {Body}", response.StatusCode, channel, body);
                return new ZavuSendResult(false, null, body);
            }

            using var doc = JsonDocument.Parse(body);
            var messageId = doc.RootElement.GetProperty("message").GetProperty("id").GetString();
            return new ZavuSendResult(true, messageId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error llamando a Zavu ({Channel})", channel);
            return new ZavuSendResult(false, null, ex.Message);
        }
    }
}