namespace DSAGrind.Common.Configuration;

public class KafkaSettings
{
    public const string SectionName = "KafkaSettings";

    public string BootstrapServers { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SecurityProtocol { get; set; } = "SASL_SSL";
    public string SaslMechanism { get; set; } = "PLAIN";
    public string GroupId { get; set; } = "dsagrind-consumer-group";
    public bool EnableAutoCommit { get; set; } = false;
    public int SessionTimeoutMs { get; set; } = 30000;
    public int HeartbeatIntervalMs { get; set; } = 3000;
    public int MaxPollRecords { get; set; } = 500;
    public string AutoOffsetReset { get; set; } = "earliest";
    public int RequestTimeoutMs { get; set; } = 30000;
    public int RetryBackoffMs { get; set; } = 100;
    public int MessageMaxBytes { get; set; } = 1000000;
    public int CompressionType { get; set; } = 0; // 0 = none, 1 = gzip, 2 = snappy, 3 = lz4
    public bool EnableIdempotence { get; set; } = true;
    public int Acks { get; set; } = -1; // all in-sync replicas
    public int Retries { get; set; } = int.MaxValue;
    public int MaxInFlightRequestsPerConnection { get; set; } = 5;
}

public class KafkaTopics
{
    public const string UserEvents = "user-events";
    public const string ProblemEvents = "problem-events";
    public const string SubmissionEvents = "submission-events";
    public const string PaymentEvents = "payment-events";
    public const string NotificationEvents = "notification-events";
    public const string AuditEvents = "audit-events";
}