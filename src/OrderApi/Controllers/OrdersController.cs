using Microsoft.AspNetCore.Mvc;
using OrderApi.DTOs;
using OrderApi.Services;
using Microsoft.Extensions.Options;
using OrderApi.Messaging;
using Shared.Contracts;
using OrderApi.Middleware;


namespace OrderApi.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    private readonly IMessagePublisher _publisher;
    private readonly ServiceBusOptions _sb;

    public OrdersController(
    IOrderService orders,
    IMessagePublisher publisher,
    IOptions<ServiceBusOptions> sbOptions)
    {
        _orders = orders;
        _publisher = publisher;
        _sb = sbOptions.Value;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? customerEmail, CancellationToken ct)
    {
        var result = await _orders.SearchAsync(status, customerEmail, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _orders.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto request, CancellationToken ct)
    {
        var created = await _orders.CreateAsync(request, ct);

        var correlationId = HttpContext.GetCorrelationId();

        var evt = new OrderCreated(
            OrderId: created.Id,
            CustomerEmail: created.CustomerEmail,
            TotalAmount: created.TotalAmount,
            Items: created.Items.Select(i => new OrderCreatedItem(i.Sku, i.Quantity, i.UnitPrice)).ToList(),
            CorrelationId: correlationId,
            CreatedAtUtc: DateTime.UtcNow
        );

        // MessageId = OrderId => processor can idempotently ignore duplicates easily
        await _publisher.PublishAsync(_sb.QueueName, evt, messageId: created.Id.ToString("N"), correlationId: correlationId, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(CancelOrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _orders.CancelAsync(id, ct);
        return Ok(result);
    }
}
