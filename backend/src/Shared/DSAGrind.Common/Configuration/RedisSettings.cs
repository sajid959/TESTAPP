namespace DSAGrind.Common.Configuration;

public class RedisSettings
{
    public const string SectionName = "RedisSettings";

    public string ConnectionString { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Database { get; set; } = 0;
    public int ConnectTimeoutSeconds { get; set; } = 30;
    public int SyncTimeoutSeconds { get; set; } = 30;
    public int CommandTimeoutSeconds { get; set; } = 30;
    public bool AbortOnConnectFail { get; set; } = false;
    public bool AllowAdmin { get; set; } = false;
    public int ConnectRetryCount { get; set; } = 3;
    public int KeepAliveSeconds { get; set; } = 60;
    public string DefaultKeyPrefix { get; set; } = "dsagrind:";
    
    // Cache expiration times
    public int UserCacheExpirationMinutes { get; set; } = 30;
    public int ProblemCacheExpirationMinutes { get; set; } = 60;
    public int CategoryCacheExpirationMinutes { get; set; } = 120;
    public int SubmissionCacheExpirationMinutes { get; set; } = 15;
    
    // Rate limiting
    public int RateLimitWindowMinutes { get; set; } = 1;
    public int LoginAttemptsLimit { get; set; } = 5;
    public int SubmissionAttemptsLimit { get; set; } = 10;
    public int ApiRequestsLimit { get; set; } = 100;
}