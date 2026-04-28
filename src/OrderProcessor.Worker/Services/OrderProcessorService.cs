using Microsoft.EntityFrameworkCore;
using OrderProcessor.Worker.Persistence;
using OrderProcessor.Worker.Persistence.Entities;
using Shared.Contracts;

namespace OrderProcessor.Worker.Services;

public sealed class OrderProcessorService : IOrderProcessorService
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderProcessorService> _logger;

    public OrderProcessorService(AppDbContext db, ILogger<OrderProcessorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ProcessAsync(OrderCreated evt, string messageId, string correlationId, CancellationToken ct)
    {
        // Load order from DB (source of truth)
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == evt.OrderId, ct);

        if (order is null)
        {
            // Nothing to process: complete message so it doesn’t loop forever.
            _logger.LogWarning("Order not found in DB. OrderId={OrderId}", evt.OrderId);
            return;
        }

        if (order.Status.Equals(OrderStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Order is cancelled. Skipping processing. OrderId={OrderId}", order.Id);
            return;
        }

        if (order.Status.Equals(OrderStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
            order.Status.Equals(OrderStatus.Failed, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Order already final state ({Status}). Skipping. OrderId={OrderId}", order.Status, order.Id);
            return;
        }

        // Process in a transaction so inventory updates + order status are consistent.
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // Mark Processing
        order.Status = OrderStatus.Processing;
        await _db.SaveChangesAsync(ct);

        // Validate inventory availability
        var skus = order.Items.Select(i => i.Sku).Distinct().ToList();
        var inventory = await _db.InventoryItems.Where(x => skus.Contains(x.Sku)).ToListAsync(ct);

        foreach (var item in order.Items)
        {
            var inv = inventory.FirstOrDefault(x => x.Sku == item.Sku);
            if (inv is null || inv.AvailableQty < item.Quantity)
            {
                order.Status = OrderStatus.Failed;
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogWarning("Insufficient inventory. OrderId={OrderId} Sku={Sku}", order.Id, item.Sku);
                return;
            }
        }

        // Reserve inventory (decrement)
        foreach (var item in order.Items)
        {
            var inv = inventory.First(x => x.Sku == item.Sku);
            inv.AvailableQty -= item.Quantity;
        }

        // Deterministic payment stub:
        // success if first byte of GUID is even, else fail (stable + predictable)
        var paymentOk = IsPaymentSuccessful(order.Id);

        if (!paymentOk)
        {
            // If payment fails, revert reservation (simple rollback by not committing tx)
            order.Status = OrderStatus.Failed;
            await _db.SaveChangesAsync(ct);
            await tx.RollbackAsync(ct);

            _logger.LogWarning("Payment failed. OrderId={OrderId}", order.Id);
            return;
        }

        order.Status = OrderStatus.Completed;

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        _logger.LogInformation("Order completed. OrderId={OrderId}", order.Id);
    }

    private static bool IsPaymentSuccessful(Guid orderId)
    {
        var bytes = orderId.ToByteArray();
        return (bytes[0] % 2) == 0;
    }
}
