namespace DSAGrind.Common.Services;

public interface IKafkaService
{
    Task PublishAsync<T>(string topic, T message, string? key = null, CancellationToken cancellationToken = default);
    Task PublishAsync<T>(string topic, T message, Dictionary<string, string>? headers, string? key = null, CancellationToken cancellationToken = default);
    Task StartConsumerAsync<T>(string topic, string consumerGroup, Func<T, Dictionary<string, string>?, Task> messageHandler, CancellationToken cancellationToken = default);
    Task StopConsumerAsync(CancellationToken cancellationToken = default);
}

public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, T message, string? key = null, CancellationToken cancellationToken = default);
    Task PublishAsync<T>(string topic, T message, Dictionary<string, string>? headers, string? key = null, CancellationToken cancellationToken = default);
}

public interface IKafkaConsumer
{
    Task StartAsync<T>(string topic, string consumerGroup, Func<T, Dictionary<string, string>?, Task> messageHandler, CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}