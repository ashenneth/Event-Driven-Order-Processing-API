using Shared.Contracts;

namespace OrderProcessor.Worker.Services;

public interface IOrderProcessorService
{
    Task ProcessAsync(OrderCreated evt, string messageId, string correlationId, CancellationToken ct);
}
