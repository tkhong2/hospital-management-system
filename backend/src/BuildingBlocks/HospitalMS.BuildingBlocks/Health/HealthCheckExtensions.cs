using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HospitalMS.BuildingBlocks.Health;

public static class HealthCheckExtensions
{
    public static IEndpointRouteBuilder MapHospitalHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        // Liveness probe: chỉ kiểm tra process sống
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false, // Không chạy check phụ thuộc
            ResponseWriter = WriteHealthResponseAsync
        });

        // Readiness probe: kiểm tra toàn bộ dependencies (DB, Redis, Broker)
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteHealthResponseAsync
        });

        // Backward compatibility
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteHealthResponseAsync
        });

        return endpoints;
    }

    private static Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds + "ms",
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description ?? (e.Value.Status == HealthStatus.Healthy ? "Service dependency is operational" : "Dependency failure"),
                duration = e.Value.Duration.TotalMilliseconds + "ms",
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}

