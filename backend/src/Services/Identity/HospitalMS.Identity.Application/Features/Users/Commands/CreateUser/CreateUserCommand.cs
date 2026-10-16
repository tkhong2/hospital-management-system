using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using HospitalMS.Identity.Domain.Entities;
using HospitalMS.Identity.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Username,
    string Password,
    string FullName,
    string Email,
    string? PhoneNumber,
    Guid? DepartmentId,
    List<Guid> RoleIds
) : IRequest<Result<Guid>>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class CreateUserCommandHandler(
    IIdentityDbContext dbContext,
    IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var usernameExists = await dbContext.Users
            .AnyAsync(u => u.Username.ToLower() == request.Username.ToLower() && !u.IsDeleted, cancellationToken);

        if (usernameExists)
        {
            return Result.Failure<Guid>("Tên đăng nhập đã tồn tại trong hệ thống.");
        }

        var emailExists = await dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<Guid>("Địa chỉ email đã được sử dụng.");
        }

        var user = new User
        {
            Username = request.Username.Trim(),
            PasswordHash = passwordHasher.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PhoneNumber = request.PhoneNumber,
            DepartmentId = request.DepartmentId,
            Status = UserStatus.Active
        };

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

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}

