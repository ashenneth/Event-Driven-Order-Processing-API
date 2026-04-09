namespace OrderApi.Messaging;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    public string ConnectionString { get; set; } = string.Empty; // local via user-secrets
    public string QueueName { get; set; } = "orders-created";

    // basic retry defaults (safe + simple)
    public int MaxRetries { get; set; } = 5;
    public int DelaySeconds { get; set; } = 1;
    public int MaxDelaySeconds { get; set; } = 10;
}
