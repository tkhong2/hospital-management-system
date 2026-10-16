using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using HospitalMS.Identity.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Username, string Password, string? IpAddress = null)
    : IRequest<Result<LoginResponseDto>>;

public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfoDto User
);

public record UserInfoDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    string? PhoneNumber,
    Guid? DepartmentId,
    string? DepartmentName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> AccessibleApps
);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
            .MinimumLength(3).WithMessage("Tên đăng nhập tối thiểu 3 ký tự.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu tối thiểu 6 ký tự.");
    }
}

public class LoginCommandHandler(
    IIdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
    : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.Department)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RoleApplications)
                        .ThenInclude(ra => ra.Application)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower() && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            return Result.Failure<LoginResponseDto>("Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<LoginResponseDto>("Tài khoản của bạn đã bị khóa hoặc ngừng hoạt động.");
        }

        if (user.IsLockedOut)
        {
            var remainingTime = user.LockoutEnd!.Value.Subtract(DateTime.UtcNow);
            var minutes = Math.Max(1, (int)Math.Ceiling(remainingTime.TotalMinutes));
            return Result.Failure<LoginResponseDto>($"Tài khoản tạm thời bị khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau {minutes} phút.");
        }

        var isPasswordValid = passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            user.RecordFailedLogin(maxFailedAttempts: 5, lockoutMinutes: 15);
            await dbContext.SaveChangesAsync(cancellationToken);

            if (user.IsLockedOut)
            {
                return Result.Failure<LoginResponseDto>("Tài khoản đã bị tạm khóa 15 phút do đăng nhập sai 5 lần liên tiếp.");
            }

            var attemptsLeft = 5 - user.AccessFailedCount;
            return Result.Failure<LoginResponseDto>($"Mật khẩu không chính xác. Bạn còn {attemptsLeft} lần thử.");
        }

        user.RecordLoginSuccess();

        // Collect distinct roles, permissions, and applications
        var roles = user.UserRoles
            .Select(ur => ur.Role.Code)
            .Distinct()
            .ToList();

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var applications = user.UserRoles
            .SelectMany(ur => ur.Role.RoleApplications)
            .Where(ra => ra.Application.IsActive)
            .Select(ra => ra.Application.Code)
            .Distinct()
            .ToList();

        var (accessToken, accessExpiresAt) = tokenService.GenerateAccessToken(user, roles, permissions);
        var (refreshTokenString, refreshExpiresAt) = tokenService.GenerateRefreshToken();

        var newRefreshToken = user.AddRefreshToken(refreshTokenString, refreshExpiresAt, request.IpAddress);
        dbContext.RefreshTokens.Add(newRefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var userInfo = new UserInfoDto(
            Id: user.Id,
            Username: user.Username,
            FullName: user.FullName,
            Email: user.Email,
            PhoneNumber: user.PhoneNumber,
            DepartmentId: user.DepartmentId,
            DepartmentName: user.Department?.Name,
            Roles: roles,
            Permissions: permissions,
            AccessibleApps: applications
        );

        var response = new LoginResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshTokenString,
            ExpiresAt: accessExpiresAt,
            User: userInfo
        );

        return Result.Success(response);
    }
}
