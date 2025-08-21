namespace DSAGrind.Common.Configuration;

public class MongoDbSettings
{
    public const string SectionName = "MongoDbSettings";

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public int MaxConnectionPoolSize { get; set; } = 100;
    public int MinConnectionPoolSize { get; set; } = 10;
    public int MaxConnectionIdleTimeMinutes { get; set; } = 30;
    public int MaxConnectionLifeTimeMinutes { get; set; } = 30;
    public int ConnectTimeoutSeconds { get; set; } = 30;
    public int SocketTimeoutSeconds { get; set; } = 0;
    public int ServerSelectionTimeoutSeconds { get; set; } = 30;
    public bool RetryWrites { get; set; } = true;
    public int RetryReads { get; set; } = 1;
}