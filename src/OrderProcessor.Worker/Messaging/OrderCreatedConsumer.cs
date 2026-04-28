using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderProcessor.Worker.Persistence;
using OrderProcessor.Worker.Services;
using Shared.Contracts;
using Shared.Observability;

namespace OrderProcessor.Worker.Messaging;

public sealed class OrderCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly ServiceBusOptions _options;

    private ServiceBusClient? _client;
    private ServiceBusProcessor? _processor;

    public OrderCreatedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<ServiceBusOptions> options,
        ILogger<OrderCreatedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            throw new InvalidOperationException("ServiceBus:ConnectionString is not configured.");

        _client = new ServiceBusClient(_options.ConnectionString);

        _processor = _client.CreateProcessor(_options.QueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 2
        });

        _processor.ProcessMessageAsync += OnMessageAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;

        _logger.LogInformation("Starting Service Bus processor for queue {Queue}", _options.QueueName);
        await _processor.StartProcessingAsync(stoppingToken);

        // keep service alive
        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        var msg = args.Message;

        var correlationId =
            !string.IsNullOrWhiteSpace(msg.CorrelationId) ? msg.CorrelationId :
            msg.ApplicationProperties.TryGetValue(Correlation.PropertyName, out var v) ? v?.ToString() ?? "" : "";

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["MessageId"] = msg.MessageId,
            ["CorrelationId"] = correlationId,
            ["Subject"] = msg.Subject
        });

        try
        {
            if (msg.DeliveryCount >= _options.MaxDeliveryAttempts)
            {
                _logger.LogWarning("Dead-lettering message due to DeliveryCount={DeliveryCount}", msg.DeliveryCount);
                await args.DeadLetterMessageAsync(msg,
                    deadLetterReason: "MaxDeliveryAttemptsExceeded",
                    deadLetterErrorDescription: $"DeliveryCount={msg.DeliveryCount}",
                    cancellationToken: args.CancellationToken);
                return;
            }

            var evt = msg.Body.ToObjectFromJson<OrderCreated>(EventJson.Options);

            if (evt is null)
            {
                _logger.LogWarning("Invalid or null OrderCreated payload. Dead-lettering message.");

                await args.DeadLetterMessageAsync(msg,
                    deadLetterReason: "InvalidPayload",
                    deadLetterErrorDescription: "OrderCreated payload was null.",
                    cancellationToken: args.CancellationToken);

                return;
            }

            using var serviceScope = _scopeFactory.CreateScope();
            var db = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var processor = serviceScope.ServiceProvider.GetRequiredService<IOrderProcessorService>();

            // Idempotency: if message already processed, complete and exit
            var already = await db.ProcessedMessages.AnyAsync(x => x.MessageId == msg.MessageId, args.CancellationToken);
            if (already)
            {
                _logger.LogInformation("Message already processed. Completing.");
                await args.CompleteMessageAsync(msg, args.CancellationToken);
                return;
            }

            await processor.ProcessAsync(evt, msg.MessageId, correlationId, args.CancellationToken);

            // record idempotency marker after successful processing
            db.ProcessedMessages.Add(new Persistence.Entities.ProcessedMessage
            {
                MessageId = msg.MessageId,
                OrderId = evt.OrderId,
                CorrelationId = correlationId,
                ProcessedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync(args.CancellationToken);

            await args.CompleteMessageAsync(msg, args.CancellationToken);
            _logger.LogInformation("Processed and completed message.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed processing message. Abandoning for retry.");
            await args.AbandonMessageAsync(msg, cancellationToken: args.CancellationToken);
        }
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus error. Entity={EntityPath} Source={ErrorSource}",
            args.EntityPath, args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }

        if (_client is not null)
            await _client.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}
