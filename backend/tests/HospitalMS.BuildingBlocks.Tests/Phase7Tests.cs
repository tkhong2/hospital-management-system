using System.Security.Claims;
using FluentAssertions;
using HospitalMS.BuildingBlocks.Audit;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.BuildingBlocks.Observability;
using HospitalMS.BuildingBlocks.Outbox;
using HospitalMS.BuildingBlocks.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HospitalMS.BuildingBlocks.Tests;

public record SampleTestIntegrationEvent(Guid AggregateId, string Action) : IntegrationEvent;

public class Phase7Tests
{
    [Fact]
    public async Task AuditLogService_ShouldRecordAndRetrieveLogs()
    {
        var auditService = new DefaultAuditLogService(NullLogger<DefaultAuditLogService>.Instance);
        var entry = new AuditLog
        {
            UserId = "user-1",
            UserName = "doctor_minh",
            Action = "CREATE_PRESCRIPTION",
            EntityName = "Prescription",
            EntityId = Guid.NewGuid().ToString(),
            DepartmentId = "KKB",
            WarehouseId = "KHO-NOITRU",
            IpAddress = "192.168.1.100"
        };

        await auditService.LogAsync(entry);
        var recentLogs = await auditService.GetRecentLogsAsync(10);

        recentLogs.Should().NotBeEmpty();
        recentLogs.Should().Contain(l => l.Action == "CREATE_PRESCRIPTION" && l.DepartmentId == "KKB");
    }

    [Fact]
    public async Task OutboxService_ShouldEnqueueAndProcessEvents()
    {
        var outboxService = new InMemoryOutboxService(NullLogger<InMemoryOutboxService>.Instance);
        var integrationEvent = new SampleTestIntegrationEvent(Guid.NewGuid(), "PATIENT_ADMITTED");

        await outboxService.EnqueueAsync(integrationEvent);
        var pending = await outboxService.GetPendingMessagesAsync(10);

        pending.Should().ContainSingle(m => m.Id == integrationEvent.EventId);
        pending[0].IsProcessed.Should().BeFalse();

        await outboxService.MarkAsProcessedAsync(integrationEvent.EventId);
        var afterProcessed = await outboxService.GetPendingMessagesAsync(10);
        afterProcessed.Should().NotContain(m => m.Id == integrationEvent.EventId);
    }

    [Fact]
    public async Task OutboxService_ShouldTrackFailedRetries()
    {
        var outboxService = new InMemoryOutboxService(NullLogger<InMemoryOutboxService>.Instance);
        var integrationEvent = new SampleTestIntegrationEvent(Guid.NewGuid(), "PHARMACY_STOCK_SYNC");

        await outboxService.EnqueueAsync(integrationEvent);
        await outboxService.MarkAsFailedAsync(integrationEvent.EventId, "Broker timeout");

        var pending = await outboxService.GetPendingMessagesAsync(10);
        var item = pending.FirstOrDefault(m => m.Id == integrationEvent.EventId);

        item.Should().NotBeNull();
        item!.RetryCount.Should().Be(1);
        item.Error.Should().Be("Broker timeout");
    }

    [Fact]
    public async Task DepartmentScopeHandler_ShouldAllowAdminAccess()
    {
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var handler = new DepartmentScopeHandler(httpContextAccessor.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("role", "Admin")
        }, "TestAuth"));

        var requirement = new DepartmentScopeRequirement("KHOA_NGOAI");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task DepartmentScopeHandler_ShouldDenyUnauthorizedDepartment()
    {
        var httpContext = new DefaultHttpContext();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var handler = new DepartmentScopeHandler(httpContextAccessor.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("department", "KHOA_NOI")
        }, "TestAuth"));

        var requirement = new DepartmentScopeRequirement("KHOA_NGOAI");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task WarehouseScopeHandler_ShouldAllowUserWithMatchingWarehouse()
    {
        var httpContext = new DefaultHttpContext();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var handler = new WarehouseScopeHandler(httpContextAccessor.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("warehouse", "KHO_CHAN")
        }, "TestAuth"));

        var requirement = new WarehouseScopeRequirement("KHO_CHAN");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public void HospitalMetrics_ShouldIncrementCountersWithoutException()
    {
        var act = () =>
        {
            HospitalMetrics.OutpatientEncounters.Add(1);
            HospitalMetrics.PrescriptionsCreated.Add(1);
            HospitalMetrics.ActiveBedsOccupied.Add(1);
            HospitalMetrics.ActiveBedsOccupied.Add(-1);
        };

        act.Should().NotThrow();
    }
}
