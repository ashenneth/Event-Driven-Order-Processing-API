using OrderApi.DTOs;
using OrderApi.Persistence.Entities;
using OrderApi.Repositories;
using OrderApi.Services.Exceptions;
using OrderApi.Services.Validation;

namespace OrderApi.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;

    public OrderService(IOrderRepository orders)
    {
        _orders = orders;
    }

    public async Task<List<OrderResponseDto>> SearchAsync(string? status, string? customerEmail, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(status) && !OrderStatus.All.Contains(status))
            throw new ValidationException("Validation failed.", new Dictionary<string, string[]>
            {
                ["status"] = new[] { "Invalid status filter." }
            });

        var list = await _orders.SearchAsync(status, customerEmail, ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<OrderResponseDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(id, ct);
        if (order is null) throw new NotFoundException("Order not found.");
        return ToDto(order);
    }

    public async Task<OrderResponseDto> CreateAsync(CreateOrderRequestDto request, CancellationToken ct)
    {
        OrderRequestValidator.Validate(request);

        var now = DateTime.UtcNow;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerEmail = request.CustomerEmail.Trim(),
            Status = OrderStatus.Pending,
            TotalAmount = request.TotalAmount,
            CreatedAt = now,
            Items = request.Items.Select(i => new OrderItem
            {
                Sku = i.Sku.Trim(),
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        await _orders.AddAsync(order, ct);
        await _orders.SaveChangesAsync(ct);

        return ToDto(order);
    }

    public async Task<CancelOrderResponseDto> CancelAsync(Guid id, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(id, ct);
        if (order is null) throw new NotFoundException("Order not found.");

        if (order.Status.Equals(OrderStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
            order.Status.Equals(OrderStatus.Failed, StringComparison.OrdinalIgnoreCase) ||
            order.Status.Equals(OrderStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException($"Order cannot be cancelled because it is {order.Status}.");
        }

        order.Status = OrderStatus.Cancelled;
        await _orders.SaveChangesAsync(ct);

        return new CancelOrderResponseDto(order.Id, order.Status);
    }

    private static OrderResponseDto ToDto(Order o) =>
        new(
            o.Id,
            o.CustomerEmail,
            o.Status,
            o.TotalAmount,
            o.CreatedAt,
            o.Items.Select(i => new OrderItemResponseDto(i.Id, i.Sku, i.Quantity, i.UnitPrice)).ToList()
        );
}
