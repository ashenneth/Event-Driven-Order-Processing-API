using System.ComponentModel.DataAnnotations;

namespace OrderProcessor.Worker.Persistence.Entities;

public class Order
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(320)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string Status { get; set; } = OrderStatus.Pending;

    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}
