using System.ComponentModel.DataAnnotations;

namespace OrderProcessor.Worker.Persistence.Entities;

public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid OrderId { get; set; }

    [Required, MaxLength(64)]
    public string Sku { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Order? Order { get; set; }
}
