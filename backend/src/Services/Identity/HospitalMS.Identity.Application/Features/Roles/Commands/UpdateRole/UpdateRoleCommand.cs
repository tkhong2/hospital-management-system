using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using HospitalMS.Identity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Roles.Commands.UpdateRole;

public record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description,
    List<Guid> PermissionIds,
    List<Guid> ApplicationIds
) : IRequest<Result>;

public class UpdateRoleCommandHandler(IIdentityDbContext dbContext)
    : IRequestHandler<UpdateRoleCommand, Result>
{
    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .Include(r => r.RolePermissions)
            .Include(r => r.RoleApplications)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (role is null)
        {
            return Result.Failure("Không tìm thấy vai trò.");
        }

        role.Name = request.Name.Trim();
        role.Description = request.Description;

        // Sync permissions
        role.RolePermissions.Clear();
        if (request.PermissionIds.Count > 0)
        {
            var permissions = await dbContext.Permissions
                .Where(p => request.PermissionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            foreach (var permission in permissions)
            {
                role.RolePermissions.Add(new RolePermission { Role = role, Permission = permission });
            }
        }

        // Sync applications
        role.RoleApplications.Clear();
        if (request.ApplicationIds.Count > 0)
        {
            var apps = await dbContext.Applications
                .Where(a => request.ApplicationIds.Contains(a.Id) && a.IsActive && !a.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var app in apps)
            {
                role.RoleApplications.Add(new RoleApplication { Role = role, Application = app });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

