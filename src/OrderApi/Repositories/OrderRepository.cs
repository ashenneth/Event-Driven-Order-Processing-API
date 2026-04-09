using Microsoft.EntityFrameworkCore;
using OrderApi.Persistence;
using OrderApi.Persistence.Entities;

namespace OrderApi.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<List<Order>> SearchAsync(string? status, string? customerEmail, CancellationToken ct)
    {
        var q = _db.Orders.Include(o => o.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(customerEmail))
            q = q.Where(o => o.CustomerEmail.Contains(customerEmail));

        return q.OrderByDescending(o => o.CreatedAt).ToListAsync(ct);
    }

    public Task AddAsync(Order order, CancellationToken ct) =>
        _db.Orders.AddAsync(order, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
