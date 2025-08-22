using Microsoft.Extensions.Logging;

namespace DSAGrind.Common.Services;

public class EventPublisher : IEventPublisher
{
    private readonly IKafkaService _kafkaService;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(IKafkaService kafkaService, ILogger<EventPublisher> logger)
    {
        _kafkaService = kafkaService;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string topic, T @event, CancellationToken cancellationToken = default) where T : class
    {
        await PublishAsync(topic, @event, null, null, cancellationToken);
    }

    public async Task PublishAsync<T>(string topic, T @event, string? key = null, CancellationToken cancellationToken = default) where T : class
    {
        await PublishAsync(topic, @event, null, key, cancellationToken);
    }

    public async Task PublishAsync<T>(string topic, T @event, Dictionary<string, string>? headers, string? key = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            _logger.LogDebug("Publishing event {EventType} to topic {Topic}", typeof(T).Name, topic);
            await _kafkaService.PublishAsync(topic, @event, headers, key, cancellationToken);
            _logger.LogDebug("Successfully published event {EventType} to topic {Topic}", typeof(T).Name, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to topic {Topic}", typeof(T).Name, topic);
            throw;
        }
    }
}