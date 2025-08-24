using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DSAGrind.Common.Configuration;

namespace DSAGrind.Common.Services;

public class KafkaService : IKafkaService, IDisposable
{
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<KafkaService> _logger;
    private readonly IProducer<string, string> _producer;
    private IConsumer<string, string>? _consumer;
    private CancellationTokenSource? _consumerCancellationTokenSource;

    public KafkaService(IOptions<KafkaSettings> kafkaSettings, ILogger<KafkaService> logger)
    {
        _kafkaSettings = kafkaSettings.Value;
        _logger = logger;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            SecurityProtocol = Enum.Parse<SecurityProtocol>(_kafkaSettings.SecurityProtocol),
            SaslMechanism = Enum.Parse<SaslMechanism>(_kafkaSettings.SaslMechanism),
            SaslUsername = _kafkaSettings.Username,
            SaslPassword = _kafkaSettings.Password,
            EnableIdempotence = _kafkaSettings.EnableIdempotence,
            Acks = (Acks)_kafkaSettings.Acks,
            MessageSendMaxRetries = _kafkaSettings.Retries,
            MaxInFlight = _kafkaSettings.MaxInFlightRequestsPerConnection,
            MessageMaxBytes = _kafkaSettings.MessageMaxBytes,
            RequestTimeoutMs = _kafkaSettings.RequestTimeoutMs,
            RetryBackoffMs = _kafkaSettings.RetryBackoffMs
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishAsync<T>(string topic, T message, string? key = null, CancellationToken cancellationToken = default)
    {
        await PublishAsync(topic, message, null, key, cancellationToken);
    }

    public async Task PublishAsync<T>(string topic, T message, Dictionary<string, string>? headers, string? key = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var messageJson = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var kafkaMessage = new Message<string, string>
            {
                Key = key ?? Guid.NewGuid().ToString(),
                Value = messageJson,
                Timestamp = new Timestamp(DateTime.UtcNow)
            };

            if (headers != null)
            {
                kafkaMessage.Headers = new Headers();
                foreach (var header in headers)
                {
                    kafkaMessage.Headers.Add(header.Key, System.Text.Encoding.UTF8.GetBytes(header.Value));
                }
            }

            var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
            
            _logger.LogDebug("Message published to Kafka topic {Topic} at offset {Offset}", 
                topic, deliveryResult.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to Kafka topic {Topic}", topic);
            throw;
        }
    }

    public Task StartConsumerAsync<T>(string topic, string consumerGroup, Func<T, Dictionary<string, string>?, Task> messageHandler, CancellationToken cancellationToken = default)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            SecurityProtocol = Enum.Parse<SecurityProtocol>(_kafkaSettings.SecurityProtocol),
            SaslMechanism = Enum.Parse<SaslMechanism>(_kafkaSettings.SaslMechanism),
            SaslUsername = _kafkaSettings.Username,
            SaslPassword = _kafkaSettings.Password,
            GroupId = consumerGroup,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_kafkaSettings.AutoOffsetReset),
            EnableAutoCommit = _kafkaSettings.EnableAutoCommit,
            SessionTimeoutMs = _kafkaSettings.SessionTimeoutMs,
            HeartbeatIntervalMs = _kafkaSettings.HeartbeatIntervalMs,
            MaxPollIntervalMs = _kafkaSettings.MaxPollRecords * 1000
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        _consumer.Subscribe(topic);

        _consumerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                while (!_consumerCancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(_consumerCancellationTokenSource.Token);
                        
                        if (consumeResult?.Message?.Value != null)
                        {
                            var message = JsonSerializer.Deserialize<T>(consumeResult.Message.Value, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });

                            if (message != null)
                            {
                                var headers = new Dictionary<string, string>();
                                if (consumeResult.Message.Headers != null)
                                {
                                    foreach (var header in consumeResult.Message.Headers)
                                    {
                                        headers[header.Key] = System.Text.Encoding.UTF8.GetString(header.GetValueBytes());
                                    }
                                }

                                await messageHandler(message, headers);
                                
                                if (!_kafkaSettings.EnableAutoCommit)
                                {
                                    _consumer.Commit(consumeResult);
                                }
                            }
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message from Kafka topic {Topic}", topic);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message from Kafka topic {Topic}", topic);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer for topic {Topic} was cancelled", topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in Kafka consumer for topic {Topic}", topic);
            }
        }, _consumerCancellationTokenSource.Token);

        _logger.LogInformation("Started Kafka consumer for topic {Topic} with consumer group {ConsumerGroup}", topic, consumerGroup);
        return Task.CompletedTask;
    }

    public Task StopConsumerAsync(CancellationToken cancellationToken = default)
    {
        if (_consumerCancellationTokenSource != null)
        {
            _consumerCancellationTokenSource.Cancel();
            _consumerCancellationTokenSource.Dispose();
            _consumerCancellationTokenSource = null;
        }

        if (_consumer != null)
        {
            _consumer.Close();
            _consumer.Dispose();
            _consumer = null;
        }

        _logger.LogInformation("Stopped Kafka consumer");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _consumerCancellationTokenSource?.Cancel();
        _consumerCancellationTokenSource?.Dispose();
        _consumer?.Close();
        _consumer?.Dispose();
        _producer?.Dispose();
    }
}