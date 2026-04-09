using OrderApi.Persistence.Entities;

namespace OrderApi.Repositories;

public interface IInventoryRepository
{
    Task<List<InventoryItem>> GetAllAsync(CancellationToken ct);
    Task<InventoryItem?> GetBySkuAsync(string sku, CancellationToken ct);
    Task AddRangeAsync(IEnumerable<InventoryItem> items, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
