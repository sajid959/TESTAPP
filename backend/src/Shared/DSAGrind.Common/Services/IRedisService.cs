namespace DSAGrind.Common.Services;

public interface IRedisService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);
    Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);
    Task<bool> ExpireAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task<TimeSpan?> GetTtlAsync(string key, CancellationToken cancellationToken = default);
    
    // Set operations
    Task<bool> SetAddAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    Task<bool> SetRemoveAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    Task<bool> SetContainsAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    Task<T[]> SetMembersAsync<T>(string key, CancellationToken cancellationToken = default);
    
    // Hash operations
    Task<bool> HashSetAsync<T>(string key, string field, T value, CancellationToken cancellationToken = default);
    Task<T?> HashGetAsync<T>(string key, string field, CancellationToken cancellationToken = default);
    Task<bool> HashDeleteAsync(string key, string field, CancellationToken cancellationToken = default);
    Task<bool> HashExistsAsync(string key, string field, CancellationToken cancellationToken = default);
    Task<Dictionary<string, T>> HashGetAllAsync<T>(string key, CancellationToken cancellationToken = default);
    
    // List operations
    Task<long> ListPushAsync<T>(string key, T value, bool toLeft = true, CancellationToken cancellationToken = default);
    Task<T?> ListPopAsync<T>(string key, bool fromLeft = true, CancellationToken cancellationToken = default);
    Task<long> ListLengthAsync(string key, CancellationToken cancellationToken = default);
    Task<T[]> ListRangeAsync<T>(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default);
    
    // Rate limiting
    Task<bool> IsRateLimitedAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default);
    Task<RateLimitResult> CheckRateLimitAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default);
}

public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public long Count { get; set; }
    public TimeSpan ResetTime { get; set; }
    public long Remaining => Math.Max(0, Limit - Count);
    public long Limit { get; set; }
}