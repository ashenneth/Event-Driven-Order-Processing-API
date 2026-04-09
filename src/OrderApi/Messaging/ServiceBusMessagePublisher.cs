using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Shared.Observability;

namespace OrderApi.Messaging;

public sealed class ServiceBusMessagePublisher : IMessagePublisher
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusMessagePublisher> _logger;

    public ServiceBusMessagePublisher(IOptions<ServiceBusOptions> options, ILogger<ServiceBusMessagePublisher> logger)
    {
        _logger = logger;

        var o = options.Value;
        if (string.IsNullOrWhiteSpace(o.ConnectionString))
            throw new InvalidOperationException("ServiceBus:ConnectionString is not configured.");

        var clientOptions = new ServiceBusClientOptions
        {
            RetryOptions = new ServiceBusRetryOptions
            {
                Mode = ServiceBusRetryMode.Exponential,
                MaxRetries = o.MaxRetries,
                Delay = TimeSpan.FromSeconds(o.DelaySeconds),
                MaxDelay = TimeSpan.FromSeconds(o.MaxDelaySeconds),
                TryTimeout = TimeSpan.FromSeconds(30)
            }
        };

        _client = new ServiceBusClient(o.ConnectionString, clientOptions);
    }

    public async Task PublishAsync<T>(
        string queueName,
        T payload,
        string messageId,
        string correlationId,
        CancellationToken ct)
    {
        var sender = _client.CreateSender(queueName);

        // Serialize with shared options so API + Worker agree
        var body = BinaryData.FromObjectAsJson(payload, EventJson.Options);

        var msg = new ServiceBusMessage(body)
        {
            MessageId = messageId,                 // IMPORTANT for idempotency/dedupe
            CorrelationId = correlationId,
            ContentType = "application/json",
            Subject = typeof(T).Name
        };

        msg.ApplicationProperties[Correlation.PropertyName] = correlationId;

        _logger.LogInformation("Publishing message {Subject} to {Queue} (MessageId={MessageId}, CorrelationId={CorrelationId})",
            msg.Subject, queueName, msg.MessageId, correlationId);

        await sender.SendMessageAsync(msg, ct);
    }
}
