namespace OrderApi.DTOs;

public record InventoryItemResponseDto(
    int Id,
    string Sku,
    int AvailableQty
);

public record SeedInventoryRequestDto(
    List<SeedInventoryItemDto> Items
);

public record SeedInventoryItemDto(
    string Sku,
    int AvailableQty
);
