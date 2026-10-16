using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BuildingBlocks.Audit;

public class DefaultAuditLogService(ILogger<DefaultAuditLogService> logger) : IAuditLogService
{
    private static readonly ConcurrentQueue<AuditLog> AuditBuffer = new();
    private const int MaxBufferSize = 500;

    public Task LogAsync(AuditLog entry, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[AUDIT] User: {UserName} ({UserId}) | Action: {Action} | Entity: {EntityName} ({EntityId}) | Dept: {DepartmentId} | WH: {WarehouseId} | TraceId: {TraceId}",
            entry.UserName, entry.UserId, entry.Action, entry.EntityName, entry.EntityId,
            entry.DepartmentId, entry.WarehouseId, entry.TraceId);

        AuditBuffer.Enqueue(entry);
        while (AuditBuffer.Count > MaxBufferSize && AuditBuffer.TryDequeue(out _))
        {
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditLog>> GetRecentLogsAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        var logs = AuditBuffer.ToArray().Reverse().Take(count).ToList();
        return Task.FromResult<IReadOnlyList<AuditLog>>(logs);
    }
}
