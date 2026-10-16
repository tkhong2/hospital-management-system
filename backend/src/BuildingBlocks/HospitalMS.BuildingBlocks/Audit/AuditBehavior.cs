using System.Diagnostics;
using System.Text.Json;
using HospitalMS.BuildingBlocks.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BuildingBlocks.Audit;

public class AuditBehavior<TRequest, TResponse>(
    IAuditLogService auditLogService,
    ICurrentUser currentUser,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuditBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Chỉ audit các lệnh thay đổi dữ liệu (Command)
        if (!requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase) && request is not IAuditableCommand)
        {
            return await next();
        }

        var response = await next();

        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            var ip = httpContext?.Connection?.RemoteIpAddress?.ToString();
            var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;

            string action = requestName;
            string entityName = typeof(TRequest).Namespace?.Split('.').LastOrDefault() ?? "General";
            string? deptId = null;
            string? warehouseId = null;

            if (request is IAuditableCommand auditable)
            {
                action = auditable.ActionName;
                entityName = auditable.EntityName;
                deptId = auditable.DepartmentId;
                warehouseId = auditable.WarehouseId;
            }

            // Đọc department/warehouse từ Claim hoặc Header nếu có
            deptId ??= httpContext?.Request.Headers["X-Department-Id"].FirstOrDefault()
                       ?? currentUser.Permissions.FirstOrDefault(p => p.StartsWith("dept:"))?.Replace("dept:", "");
            warehouseId ??= httpContext?.Request.Headers["X-Warehouse-Id"].FirstOrDefault()
                           ?? currentUser.Permissions.FirstOrDefault(p => p.StartsWith("wh:"))?.Replace("wh:", "");

            var entry = new AuditLog
            {
                UserId = currentUser.UserId,
                UserName = currentUser.UserName,
                Action = action,
                EntityName = entityName,
                NewValues = JsonSerializer.Serialize(request, JsonOptions),
                DepartmentId = deptId,
                WarehouseId = warehouseId,
                IpAddress = ip,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };

            await auditLogService.LogAsync(entry, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write audit log for request {RequestName}", requestName);
        }

        return response;
    }
}

