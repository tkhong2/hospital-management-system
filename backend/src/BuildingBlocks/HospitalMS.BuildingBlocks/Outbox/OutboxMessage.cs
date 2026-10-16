using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.BuildingBlocks.Outbox;

public class OutboxMessage : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public bool IsProcessed => ProcessedOn.HasValue;
}

