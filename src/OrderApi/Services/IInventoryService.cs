using OrderApi.DTOs;

namespace OrderApi.Services;

public interface IInventoryService
{
    Task<List<InventoryItemResponseDto>> GetAllAsync(CancellationToken ct);
    Task<int> SeedAsync(SeedInventoryRequestDto request, CancellationToken ct);
}
