using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using DSAGrind.Common.Configuration;

namespace DSAGrind.Common.Services;

public class RedisService : IRedisService, IDisposable
{
    private readonly RedisSettings _redisSettings;
    private readonly ILogger<RedisService> _logger;
    private readonly Lazy<ConnectionMultiplexer> _connectionMultiplexer;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisService(IOptions<RedisSettings> redisSettings, ILogger<RedisService> logger)
    {
        _redisSettings = redisSettings.Value;
        _logger = logger;
        
        _connectionMultiplexer = new Lazy<ConnectionMultiplexer>(() =>
        {
            var configuration = ConfigurationOptions.Parse(_redisSettings.ConnectionString);
            configuration.Password = _redisSettings.Password;
            configuration.ConnectTimeout = _redisSettings.ConnectTimeoutSeconds * 1000;
            configuration.SyncTimeout = _redisSettings.SyncTimeoutSeconds * 1000;
            configuration.CommandMap = CommandMap.Create(new HashSet<string>(), available: false);
            configuration.CommandMap = CommandMap.Default;
            configuration.AbortOnConnectFail = _redisSettings.AbortOnConnectFail;
            configuration.AllowAdmin = _redisSettings.AllowAdmin;
            configuration.ConnectRetry = _redisSettings.ConnectRetryCount;
            configuration.KeepAlive = _redisSettings.KeepAliveSeconds;

            return ConnectionMultiplexer.Connect(configuration);
        });

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private IDatabase Database => _connectionMultiplexer.Value.GetDatabase(_redisSettings.Database);
    private string GetKey(string key) => $"{_redisSettings.DefaultKeyPrefix}{key}";

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await Database.StringGetAsync(GetKey(key));
            if (!value.HasValue)
                return default;

            if (typeof(T) == typeof(string))
                return (T)(object)value.ToString();

            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from Redis for key {Key}", key);
            return default;
        }
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            string serializedValue;
            if (typeof(T) == typeof(string))
                serializedValue = value?.ToString() ?? string.Empty;
            else
                serializedValue = JsonSerializer.Serialize(value, _jsonOptions);

            return await Database.StringSetAsync(GetKey(key), serializedValue, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in Redis for key {Key}", key);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Database.KeyDeleteAsync(GetKey(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting key from Redis {Key}", key);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Database.KeyExistsAsync(GetKey(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key exists in Redis {Key}", key);
            return false;
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Database.StringIncrementAsync(GetKey(key), value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing value in Redis for key {Key}", key);
            return 0;
        }
    }

    public async Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Database.StringDecrementAsync(GetKey(key), value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing value in Redis for key {Key}", key);
            return 0;
        }
    }

    public async Task<bool> ExpireAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Database.KeyExpireAsync(GetKey(key), expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting expiration for Redis key {Key}", key);
            return false;
        }
    }

    public async Task<TimeSpan?> GetTtlAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Database.KeyTimeToLiveAsync(GetKey(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TTL for Redis key {Key}", key);
            return null;
        }
    }

    public async Task<bool> SetAddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            return await Database.SetAddAsync(GetKey(key), serializedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to set in Redis for key {Key}", key);
            return false;
        }
    }

    public async Task<bool> SetRemoveAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            return await Database.SetRemoveAsync(GetKey(key), serializedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from set in Redis for key {Key}", key);
            return false;
        }
    }

    public async Task<bool> SetContainsAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            return await Database.SetContainsAsync(GetKey(key), serializedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking set membership in Redis for key {Key}", key);
            return false;
        }
    }

    public async Task<T[]> SetMembersAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var values = await Database.SetMembersAsync(GetKey(key));
            return values.Select(v => JsonSerializer.Deserialize<T>(v!, _jsonOptions)!).ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting set members from Redis for key {Key}", key);
            return Array.Empty<T>();
        }
    }

    public async Task<bool> HashSetAsync<T>(string key, string field, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            return await Database.HashSetAsync(GetKey(key), field, serializedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting hash field in Redis for key {Key} field {Field}", key, field);
            return false;
        }
    }

    public async Task<T?> HashGetAsync<T>(string key, string field, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await Database.HashGetAsync(GetKey(key), field);
            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hash field from Redis for key {Key} field {Field}", key, field);
            return default;
        }
    }

    public async Task<bool> HashDeleteAsync(string key, string field, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Database.HashDeleteAsync(GetKey(key), field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hash field from Redis for key {Key} field {Field}", key, field);
            return false;
        }
    }

    public async Task<bool> HashExistsAsync(string key, string field, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Database.HashExistsAsync(GetKey(key), field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking hash field existence in Redis for key {Key} field {Field}", key, field);
            return false;
        }
    }

    public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var hash = await Database.HashGetAllAsync(GetKey(key));
            var result = new Dictionary<string, T>();

            foreach (var item in hash)
            {
                result[item.Name!] = JsonSerializer.Deserialize<T>(item.Value!, _jsonOptions)!;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all hash fields from Redis for key {Key}", key);
            return new Dictionary<string, T>();
        }
    }

    public async Task<long> ListPushAsync<T>(string key, T value, bool toLeft = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            return toLeft 
                ? await Database.ListLeftPushAsync(GetKey(key), serializedValue)
                : await Database.ListRightPushAsync(GetKey(key), serializedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing to list in Redis for key {Key}", key);
            return 0;
        }
    }

    public async Task<T?> ListPopAsync<T>(string key, bool fromLeft = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = fromLeft 
                ? await Database.ListLeftPopAsync(GetKey(key))
                : await Database.ListRightPopAsync(GetKey(key));
                
            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error popping from list in Redis for key {Key}", key);
            return default;
        }
    }

    public async Task<long> ListLengthAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Database.ListLengthAsync(GetKey(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting list length from Redis for key {Key}", key);
            return 0;
        }
    }

    public async Task<T[]> ListRangeAsync<T>(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default)
    {
        try
        {
            var values = await Database.ListRangeAsync(GetKey(key), start, stop);
            return values.Select(v => JsonSerializer.Deserialize<T>(v!, _jsonOptions)!).ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting list range from Redis for key {Key}", key);
            return Array.Empty<T>();
        }
    }

    public async Task<bool> IsRateLimitedAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default)
    {
        var result = await CheckRateLimitAsync(key, limit, window, cancellationToken);
        return !result.IsAllowed;
    }

    public async Task<RateLimitResult> CheckRateLimitAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default)
    {
        try
        {
            var rateLimitKey = GetKey($"rate_limit:{key}");
            var count = await Database.StringIncrementAsync(rateLimitKey);
            
            if (count == 1)
            {
                await Database.KeyExpireAsync(rateLimitKey, window);
            }

            var ttl = await Database.KeyTimeToLiveAsync(rateLimitKey) ?? TimeSpan.Zero;

            return new RateLimitResult
            {
                IsAllowed = count <= limit,
                Count = count,
                Limit = limit,
                ResetTime = ttl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for key {Key}", key);
            return new RateLimitResult { IsAllowed = true, Count = 0, Limit = limit };
        }
    }

    public void Dispose()
    {
        if (_connectionMultiplexer.IsValueCreated)
        {
            _connectionMultiplexer.Value.Dispose();
        }
    }
}