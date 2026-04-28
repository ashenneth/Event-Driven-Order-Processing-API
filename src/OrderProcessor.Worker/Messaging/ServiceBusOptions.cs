namespace OrderProcessor.Worker.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    public string ConnectionString { get; set; } = string.Empty;
    public string QueueName { get; set; } = "orders-created";

    public int MaxDeliveryAttempts { get; set; } = 5;
}
