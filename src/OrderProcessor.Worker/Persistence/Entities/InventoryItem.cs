using System.ComponentModel.DataAnnotations;

namespace OrderProcessor.Worker.Persistence.Entities;

public class InventoryItem
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Sku { get; set; } = string.Empty;

    public int AvailableQty { get; set; }
}
