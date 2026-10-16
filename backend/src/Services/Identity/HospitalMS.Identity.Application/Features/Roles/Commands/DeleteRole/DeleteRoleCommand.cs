using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(Guid Id) : IRequest<Result>;

public class DeleteRoleCommandHandler(IIdentityDbContext dbContext)
    : IRequestHandler<DeleteRoleCommand, Result>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (role is null)
        {
            return Result.Failure("Không tìm thấy vai trò.");
        }

        if (role.IsSystemRole)
        {
            return Result.Failure("Không thể xóa vai trò hệ thống mặc định.");
        }

        if (role.UserRoles.Count > 0)
        {
            return Result.Failure($"Không thể xóa vai trò đang được gán cho {role.UserRoles.Count} người dùng.");
        }

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

