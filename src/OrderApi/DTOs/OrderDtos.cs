namespace OrderApi.DTOs;

public record OrderItemRequestDto(
    string Sku,
    int Quantity,
    decimal UnitPrice
);

public record CreateOrderRequestDto(
    string CustomerEmail,
    decimal TotalAmount,
    List<OrderItemRequestDto> Items
);

public record OrderItemResponseDto(
    int Id,
    string Sku,
    int Quantity,
    decimal UnitPrice
);

public record OrderResponseDto(
    Guid Id,
    string CustomerEmail,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    List<OrderItemResponseDto> Items
);

public record CancelOrderResponseDto(
    Guid Id,
    string Status
);
