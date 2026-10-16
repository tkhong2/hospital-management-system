using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace HospitalMS.BuildingBlocks.Security;

public class DepartmentScopeRequirement(string? requiredDepartment = null) : IAuthorizationRequirement
{
    public string? RequiredDepartment { get; } = requiredDepartment;
}

public class DepartmentScopeHandler(IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<DepartmentScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DepartmentScopeRequirement requirement)
    {
        // Admin hoặc SuperUser được phép toàn quyền truy cập tất cả các khoa
        if (context.User.IsInRole("Admin") || context.User.HasClaim("role", "Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var httpContext = httpContextAccessor.HttpContext;
        var requestedDept = requirement.RequiredDepartment
                            ?? httpContext?.Request.RouteValues["departmentCode"]?.ToString()
                            ?? httpContext?.Request.Headers["X-Department-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(requestedDept))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Kiểm tra User có Claim khoa này không
        var userDepts = context.User.FindAll("department").Select(c => c.Value).ToList();
        if (userDepts.Contains(requestedDept, StringComparer.OrdinalIgnoreCase) ||
            userDepts.Contains("*"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class WarehouseScopeRequirement(string? requiredWarehouse = null) : IAuthorizationRequirement
{
    public string? RequiredWarehouse { get; } = requiredWarehouse;
}

public class WarehouseScopeHandler(IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<WarehouseScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WarehouseScopeRequirement requirement)
    {
        if (context.User.IsInRole("Admin") || context.User.HasClaim("role", "Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var httpContext = httpContextAccessor.HttpContext;
        var requestedWh = requirement.RequiredWarehouse
                          ?? httpContext?.Request.RouteValues["warehouseId"]?.ToString()
                          ?? httpContext?.Request.Headers["X-Warehouse-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(requestedWh))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userWarehouses = context.User.FindAll("warehouse").Select(c => c.Value).ToList();
        if (userWarehouses.Contains(requestedWh, StringComparer.OrdinalIgnoreCase) ||
            userWarehouses.Contains("*"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

