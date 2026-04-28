using System.ComponentModel.DataAnnotations;

namespace OrderProcessor.Worker.Persistence.Entities;

public class ProcessedMessage
{
    [Key, MaxLength(64)]
    public string MessageId { get; set; } = string.Empty;

    public Guid OrderId { get; set; }
    [MaxLength(64)]
    public string CorrelationId { get; set; } = string.Empty;

    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}
