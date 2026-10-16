namespace HospitalMS.BuildingBlocks.Audit;

public interface IAuditLogService
{
    Task LogAsync(AuditLog entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetRecentLogsAsync(int count = 50, CancellationToken cancellationToken = default);
}

public interface IAuditableCommand
{
    string ActionName { get; }
    string EntityName { get; }
    string? DepartmentId => null;
    string? WarehouseId => null;
}
