using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Confirmo.Api.Services;

public interface IRedisQueueService
{
    Task PublishAsync(string stream, object message);
    Task<StreamEntry[]> ReadAsync(string stream, string consumerGroup, string consumerName, int count = 10, int blockMs = 5000);
    Task AckAsync(string stream, string consumerGroup, params RedisValue[] messageIds);
    Task CreateConsumerGroupAsync(string stream, string groupName);
}