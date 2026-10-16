using HospitalMS.BuildingBlocks.Abstractions;
using HospitalMS.BuildingBlocks.Audit;
using HospitalMS.BuildingBlocks.Outbox;
using HospitalMS.BuildingBlocks.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.BuildingBlocks;

public static class DependencyInjection
{
    public static IServiceCollection AddHospitalBuildingBlocks(this IServiceCollection services)
    {
        // HttpContext and User Context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        // Audit Logging
        services.AddSingleton<IAuditLogService, DefaultAuditLogService>();

        // Outbox Pattern
        services.AddSingleton<IOutboxService, InMemoryOutboxService>();

        // Scope Authorization (Khoa / Kho)
        services.AddScoped<IAuthorizationHandler, DepartmentScopeHandler>();
        services.AddScoped<IAuthorizationHandler, WarehouseScopeHandler>();

        return services;
    }
}

