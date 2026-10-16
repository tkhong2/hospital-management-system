using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Auth.Queries.GetMeApplications;

public record GetMeApplicationsQuery(Guid UserId) : IRequest<Result<UserApplicationsDto>>;

public record UserApplicationsDto(
    Guid UserId,
    string Username,
    string FullName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<ApplicationItemDto> Applications
);

public record ApplicationItemDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string Icon,
    string Color,
    string Path,
    int DisplayOrder,
    IReadOnlyList<string> Permissions
);

public class GetMeApplicationsQueryHandler(IIdentityDbContext dbContext)
    : IRequestHandler<GetMeApplicationsQuery, Result<UserApplicationsDto>>
{
    public async Task<Result<UserApplicationsDto>> Handle(GetMeApplicationsQuery request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RoleApplications)
                        .ThenInclude(ra => ra.Application)
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserApplicationsDto>("Không tìm thấy thông tin người dùng.");
        }

        var roles = user.UserRoles
            .Select(ur => ur.Role.Code)
            .Distinct()
            .ToList();

        var allPermissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        // Check if user is SuperAdmin or Admin
        var isAdmin = roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

        List<ApplicationItemDto> applicationsList;

        if (isAdmin)
        {
            // Admins get access to all active applications
            var allApps = await dbContext.Applications
                .Where(a => a.IsActive && !a.IsDeleted)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync(cancellationToken);

            applicationsList = allApps.Select(app => new ApplicationItemDto(
                Id: app.Id,
                Code: app.Code,
                Name: app.Name,
                Description: app.Description,
                Icon: app.Icon,
                Color: app.Color,
                Path: app.Path,
                DisplayOrder: app.DisplayOrder,
                Permissions: allPermissions
            )).ToList();
        }
        else
        {
            applicationsList = user.UserRoles
                .SelectMany(ur => ur.Role.RoleApplications)
                .Select(ra => ra.Application)
                .Where(a => a.IsActive && !a.IsDeleted)
                .DistinctBy(a => a.Id)
                .OrderBy(a => a.DisplayOrder)
                .Select(app => new ApplicationItemDto(
                    Id: app.Id,
                    Code: app.Code,
                    Name: app.Name,
                    Description: app.Description,
                    Icon: app.Icon,
                    Color: app.Color,
                    Path: app.Path,
                    DisplayOrder: app.DisplayOrder,
                    Permissions: allPermissions.Where(p => p.StartsWith(app.Code.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)).ToList()
                ))
                .ToList();
        }

        var result = new UserApplicationsDto(
            UserId: user.Id,
            Username: user.Username,
            FullName: user.FullName,
            Roles: roles,
            Permissions: allPermissions,
            Applications: applicationsList
        );

        return Result.Success(result);
    }
}

