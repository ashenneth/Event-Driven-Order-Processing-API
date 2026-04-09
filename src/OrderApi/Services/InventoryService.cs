using OrderApi.DTOs;
using OrderApi.Persistence.Entities;
using OrderApi.Repositories;
using OrderApi.Services.Exceptions;

namespace OrderApi.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventory;

    public InventoryService(IInventoryRepository inventory)
    {
        _inventory = inventory;
    }

    public async Task<List<InventoryItemResponseDto>> GetAllAsync(CancellationToken ct)
    {
        var items = await _inventory.GetAllAsync(ct);
        return items.Select(x => new InventoryItemResponseDto(x.Id, x.Sku, x.AvailableQty)).ToList();
    }

    public async Task<int> SeedAsync(SeedInventoryRequestDto request, CancellationToken ct)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ValidationException("Validation failed.", new Dictionary<string, string[]>
            {
                ["items"] = new[] { "Provide at least 1 inventory item." }
            });
        }

        // Simple upsert-like behavior (believable): if sku exists, update qty; else insert.
        var addedOrUpdated = 0;

        foreach (var i in request.Items)
        {
            if (string.IsNullOrWhiteSpace(i.Sku))
                continue;

            if (i.AvailableQty < 0)
                throw new ValidationException("Validation failed.", new Dictionary<string, string[]>
                {
                    [$"sku:{i.Sku}"] = new[] { "AvailableQty must be >= 0." }
                });

            var sku = i.Sku.Trim();
            var existing = await _inventory.GetBySkuAsync(sku, ct);

            if (existing is null)
            {
                await _inventory.AddRangeAsync(new[]
                {
                    new InventoryItem { Sku = sku, AvailableQty = i.AvailableQty }
                }, ct);
                addedOrUpdated++;
            }
            else
            {
                existing.AvailableQty = i.AvailableQty;
                addedOrUpdated++;
            }
        }

        await _inventory.SaveChangesAsync(ct);
        return addedOrUpdated;
    }
}
