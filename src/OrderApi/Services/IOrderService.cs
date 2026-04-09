using OrderApi.DTOs;

namespace OrderApi.Services;

public interface IOrderService
{
    Task<List<OrderResponseDto>> SearchAsync(string? status, string? customerEmail, CancellationToken ct);
    Task<OrderResponseDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<OrderResponseDto> CreateAsync(CreateOrderRequestDto request, CancellationToken ct);
    Task<CancelOrderResponseDto> CancelAsync(Guid id, CancellationToken ct);
}
