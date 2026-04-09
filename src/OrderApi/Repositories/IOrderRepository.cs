using OrderApi.Persistence.Entities;

namespace OrderApi.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Order>> SearchAsync(string? status, string? customerEmail, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
