using System.ComponentModel.DataAnnotations;

namespace OrderApi.Persistence.Entities;

public class InventoryItem
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Sku { get; set; } = string.Empty;

    [Range(0, 100000000)]
    public int AvailableQty { get; set; }
}