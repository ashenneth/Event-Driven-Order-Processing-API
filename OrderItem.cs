using System;
using System.ComponentModel.DataAnnotations;

namespace OrderApi.Persistence.Entities;

public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid OrderId { get; set; }

    [Required, MaxLength(64)]
    public string Sku { get; set; } = string.Empty;

    [Range(1, 100000)]
    public int Quantity { get; set; }

    [Range(0.0, 999999999.0)]
    public decimal UnitPrice { get; set; }

    public Order? Order { get; set; }
}