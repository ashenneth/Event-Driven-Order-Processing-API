using Microsoft.EntityFrameworkCore;
using OrderApi.Persistence;
using OrderApi.Persistence.Entities;

namespace OrderApi.Repositories;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _db;

    public InventoryRepository(AppDbContext db) => _db = db;

    public Task<List<InventoryItem>> GetAllAsync(CancellationToken ct) =>
        _db.InventoryItems.OrderBy(x => x.Sku).ToListAsync(ct);

    public Task<InventoryItem?> GetBySkuAsync(string sku, CancellationToken ct) =>
        _db.InventoryItems.FirstOrDefaultAsync(x => x.Sku == sku, ct);

    public Task AddRangeAsync(IEnumerable<InventoryItem> items, CancellationToken ct) =>
        _db.InventoryItems.AddRangeAsync(items, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
