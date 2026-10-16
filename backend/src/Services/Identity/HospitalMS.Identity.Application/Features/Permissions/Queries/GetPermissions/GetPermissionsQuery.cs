using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Permissions.Queries.GetPermissions;

public record GetPermissionsQuery : IRequest<Result<IReadOnlyList<PermissionGroupDto>>>;

public record PermissionGroupDto(
    string Module,
    IReadOnlyList<PermissionItemDto> Permissions
);

public record PermissionItemDto(
    Guid Id,
    string Code,
    string Name,
    string? Description
);

public class GetPermissionsQueryHandler(IIdentityDbContext dbContext)
    : IRequestHandler<GetPermissionsQuery, Result<IReadOnlyList<PermissionGroupDto>>>
{
    public async Task<Result<IReadOnlyList<PermissionGroupDto>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await dbContext.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Code)
            .ToListAsync(cancellationToken);

        var grouped = permissions
            .GroupBy(p => p.Module)
            .Select(g => new PermissionGroupDto(
                Module: g.Key,
                Permissions: g.Select(p => new PermissionItemDto(p.Id, p.Code, p.Name, p.Description)).ToList()
            ))
            .ToList();

        return Result.Success<IReadOnlyList<PermissionGroupDto>>(grouped);
    }
}

