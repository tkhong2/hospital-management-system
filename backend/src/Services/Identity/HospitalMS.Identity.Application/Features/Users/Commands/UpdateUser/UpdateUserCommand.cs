using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using HospitalMS.Identity.Domain.Entities;
using HospitalMS.Identity.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    Guid? DepartmentId,
    UserStatus Status,
    List<Guid> RoleIds
) : IRequest<Result>;

public class UpdateUserCommandHandler(IIdentityDbContext dbContext)
    : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Không tìm thấy thông tin người dùng.");
        }

        var emailExists = await dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.Id != request.Id && !u.IsDeleted, cancellationToken);

        if (emailExists)
        {
            return Result.Failure("Địa chỉ email đã được sử dụng bởi tài khoản khác.");
        }

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim().ToLower();
        user.PhoneNumber = request.PhoneNumber;
        user.DepartmentId = request.DepartmentId;
        user.Status = request.Status;

        // Sync Roles
        user.UserRoles.Clear();
        if (request.RoleIds.Count > 0)
        {
            var roles = await dbContext.Roles
                .Where(r => request.RoleIds.Contains(r.Id) && !r.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var role in roles)
            {
                user.UserRoles.Add(new UserRole { User = user, Role = role });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

