using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Roles.Queries.GetRoles;

public record GetRolesQuery : IRequest<Result<IReadOnlyList<RoleDto>>>;

public record RoleDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsSystemRole,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> ApplicationCodes
);

public class GetRolesQueryHandler(IIdentityDbContext dbContext)
    : IRequestHandler<GetRolesQuery, Result<IReadOnlyList<RoleDto>>>
{
    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await dbContext.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.RoleApplications)
                .ThenInclude(ra => ra.Application)
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(
                r.Id,
                r.Code,
                r.Name,
                r.Description,
                r.IsSystemRole,
                r.RolePermissions.Select(rp => rp.Permission.Code).ToList(),
                r.RoleApplications.Select(ra => ra.Application.Code).ToList()
            ))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<RoleDto>>(roles);
    }
}

