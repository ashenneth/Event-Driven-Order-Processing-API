namespace Shared.Contracts;

public sealed record OrderCreated(
    Guid OrderId,
    string CustomerEmail,
    decimal TotalAmount,
    List<OrderCreatedItem> Items,
    string CorrelationId,
    DateTime CreatedAtUtc
);

public sealed record OrderCreatedItem(
    string Sku,
    int Quantity,
    decimal UnitPrice
);
