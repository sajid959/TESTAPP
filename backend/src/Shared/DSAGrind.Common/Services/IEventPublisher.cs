namespace DSAGrind.Common.Services;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, T @event, CancellationToken cancellationToken = default) where T : class;
    Task PublishAsync<T>(string topic, T @event, string? key = null, CancellationToken cancellationToken = default) where T : class;
    Task PublishAsync<T>(string topic, T @event, Dictionary<string, string>? headers, string? key = null, CancellationToken cancellationToken = default) where T : class;
}