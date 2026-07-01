using System.Text.Json;
using StackExchange.Redis;

namespace Confirmo.Api.Services;

public class RedisQueueService : IRedisQueueService
{
    private readonly ConnectionMultiplexer _redis;
    private readonly ILogger<RedisQueueService> _logger;

    public RedisQueueService(IConfiguration config, ILogger<RedisQueueService> logger)
    {
        var connectionString = config["Redis:ConnectionString"] ?? "redis:6379";
        _redis = ConnectionMultiplexer.Connect(connectionString);
        _logger = logger;
    }

    public async Task PublishAsync(string stream, object message)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(message);
        await db.StreamAddAsync(stream, new NameValueEntry[] { new("data", json) });
        _logger.LogDebug("Publicado en {Stream}", stream);
    }

    public async Task<StreamEntry[]> ReadAsync(string stream, string consumerGroup, string consumerName, int count = 10, int blockMs = 5000)
    {
        var db = _redis.GetDatabase();

        // Asegurar que el consumer group existe
        try
        {
            await db.StreamCreateConsumerGroupAsync(stream, consumerGroup, StreamPosition.NewMessages);
        }
        catch (RedisException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Ya existe, continuar
        }

        var entries = await db.StreamReadGroupAsync(key: stream, groupName: consumerGroup, consumerName: consumerName, position: ">", count: count, noAck: false);
        return entries ?? Array.Empty<StreamEntry>();
    }

    public async Task AckAsync(string stream, string consumerGroup, params RedisValue[] messageIds)
    {
        if (messageIds.Length > 0)
        {
            var db = _redis.GetDatabase();
            await db.StreamAcknowledgeAsync(stream, consumerGroup, messageIds);
        }
    }

    public async Task CreateConsumerGroupAsync(string stream, string groupName)
    {
        var db = _redis.GetDatabase();
        try
        {
            await db.StreamCreateConsumerGroupAsync(stream, groupName, StreamPosition.NewMessages);
        }
        catch (RedisException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Ya existe
        }
    }
}