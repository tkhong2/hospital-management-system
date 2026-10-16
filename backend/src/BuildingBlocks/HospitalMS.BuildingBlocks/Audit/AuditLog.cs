using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.BuildingBlocks.Audit;

public class AuditLog : BaseEntity
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, DISPENSE, DISCHARGE...
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? DepartmentId { get; set; }
    public string? WarehouseId { get; set; }
    public string? IpAddress { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

