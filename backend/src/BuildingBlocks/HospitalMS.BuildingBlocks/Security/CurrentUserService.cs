using System.Security.Claims;
using HospitalMS.BuildingBlocks.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HospitalMS.BuildingBlocks.Security;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User?.FindFirstValue("sub") ??
        User?.FindFirstValue("id");

    public string? UserName =>
        User?.FindFirstValue(ClaimTypes.Name) ??
        User?.FindFirstValue("username");

    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email) ??
        User?.FindFirstValue("email");

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ??
        User?.FindAll("role").Select(c => c.Value).ToList() ??
        [];

    public IReadOnlyList<string> Permissions =>
        User?.FindAll("permission").Select(c => c.Value).ToList() ??
        User?.FindAll("permissions").Select(c => c.Value).ToList() ??
        [];

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public bool HasPermission(string permission) =>
        Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ||
        Roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase) ||
        Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
