namespace OrderApi.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(
        string queueName,
        T payload,
        string messageId,
        string correlationId,
        CancellationToken ct);
}
