using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using HospitalMS.Identity.Application.Features.Auth.Commands.Login;
using HospitalMS.Identity.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken, string? IpAddress = null)
    : IRequest<Result<LoginResponseDto>>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token không được để trống.");
    }
}

public class RefreshTokenCommandHandler(
    IIdentityDbContext dbContext,
    ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingRefreshToken = await dbContext.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Department)
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RoleApplications)
                            .ThenInclude(ra => ra.Application)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (existingRefreshToken is null)
        {
            return Result.Failure<LoginResponseDto>("Refresh token không hợp lệ.");
        }

        var user = existingRefreshToken.User;

        // If token has been revoked, possible token theft attempt -> revoke all user tokens
        if (existingRefreshToken.IsRevoked)
        {
            user.RevokeAllRefreshTokens(request.IpAddress);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Failure<LoginResponseDto>("Phiên đăng nhập không an toàn hoặc đã bị thu hồi. Vui lòng đăng nhập lại.");
        }

        if (existingRefreshToken.IsExpired)
        {
            return Result.Failure<LoginResponseDto>("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        }

        if (user.Status != UserStatus.Active || user.IsDeleted)
        {
            return Result.Failure<LoginResponseDto>("Tài khoản của bạn đã bị khóa.");
        }

        // Roles & Permissions
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

        // Token rotation: Revoke current refresh token and issue a new one
        var (newAccessToken, accessExpiresAt) = tokenService.GenerateAccessToken(user, roles, permissions);
        var (newRefreshTokenString, refreshExpiresAt) = tokenService.GenerateRefreshToken();

        existingRefreshToken.Revoke(request.IpAddress, newRefreshTokenString);
        var newRefreshToken = user.AddRefreshToken(newRefreshTokenString, refreshExpiresAt, request.IpAddress);
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

        return Result.Success(new LoginResponseDto(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshTokenString,
            ExpiresAt: accessExpiresAt,
            User: userInfo
        ));
    }
}
