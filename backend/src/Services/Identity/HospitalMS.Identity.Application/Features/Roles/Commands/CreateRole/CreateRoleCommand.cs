using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using HospitalMS.Identity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Roles.Commands.CreateRole;

public record CreateRoleCommand(
    string Code,
    string Name,
    string? Description,
    List<Guid> PermissionIds,
    List<Guid> ApplicationIds
) : IRequest<Result<Guid>>;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateRoleCommandHandler(IIdentityDbContext dbContext)
    : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await dbContext.Roles
            .AnyAsync(r => r.Code.ToLower() == request.Code.ToLower() && !r.IsDeleted, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<Guid>("Mã vai trò đã tồn tại.");
        }

        var role = new Role
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description,
            IsSystemRole = false
        };

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

        if (request.ApplicationIds.Count > 0)
        {
            var applications = await dbContext.Applications
                .Where(a => request.ApplicationIds.Contains(a.Id) && a.IsActive && !a.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var app in applications)
            {
                role.RoleApplications.Add(new RoleApplication { Role = role, Application = app });
            }
        }

        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(role.Id);
    }
}

