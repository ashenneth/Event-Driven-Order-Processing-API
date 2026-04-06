using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace OrderApi.Persistence.Entities;

public class Order
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(320)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string Status { get; set; } = OrderStatus.Pending;

    [Range(0, 999999999)]
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}