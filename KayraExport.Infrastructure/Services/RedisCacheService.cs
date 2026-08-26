using System.Text.Json;
using KayraExport.Application.Interfaces;
using StackExchange.Redis;

namespace KayraExport.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private static readonly TimeSpan OperationTimeout =
        TimeSpan.FromSeconds(3);

    private readonly IDatabase _database;

    public RedisCacheService(
        IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedValue = await _database
                .StringGetAsync(key)
                .WaitAsync(OperationTimeout, cancellationToken);

            if (cachedValue.IsNullOrEmpty)
            {
                Console.WriteLine($"Redis cache miss: {key}");
                return default;
            }

            Console.WriteLine($"Redis cache hit: {key}");

            return JsonSerializer.Deserialize<T>(
                cachedValue.ToString());
        }
        catch (RedisException exception)
        {
            Console.WriteLine(
                $"Redis GET error: {exception.Message}");

            return default;
        }
        catch (TimeoutException exception)
        {
            Console.WriteLine(
                $"Redis GET timeout: {exception.Message}");

            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);

            var wasSet = await _database
                .StringSetAsync(key, serializedValue, expiration)
                .WaitAsync(OperationTimeout, cancellationToken);

            Console.WriteLine(
                $"Redis SET result: Key={key}, Success={wasSet}");
        }
        catch (RedisException exception)
        {
            Console.WriteLine(
                $"Redis SET error: {exception.Message}");
        }
        catch (TimeoutException exception)
        {
            Console.WriteLine(
                $"Redis SET timeout: {exception.Message}");
        }
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var wasRemoved = await _database
                .KeyDeleteAsync(key)
                .WaitAsync(OperationTimeout, cancellationToken);

            Console.WriteLine(
                $"Redis DELETE result: Key={key}, Removed={wasRemoved}");
        }
        catch (RedisException exception)
        {
            Console.WriteLine(
                $"Redis DELETE error: {exception.Message}");
        }
        catch (TimeoutException exception)
        {
            Console.WriteLine(
                $"Redis DELETE timeout: {exception.Message}");
        }
    }
}