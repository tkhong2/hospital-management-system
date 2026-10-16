using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword)
    : IRequest<Result>;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mật khẩu hiện tại không được để trống.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu mới tối thiểu 6 ký tự.")
            .NotEqual(x => x.CurrentPassword).WithMessage("Mật khẩu mới phải khác mật khẩu hiện tại.");
    }
}

public class ChangePasswordCommandHandler(
    IIdentityDbContext dbContext,
    IPasswordHasher passwordHasher)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Không tìm thấy thông tin người dùng.");
        }

        var isOldPasswordValid = passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash);
        if (!isOldPasswordValid)
        {
            return Result.Failure("Mật khẩu hiện tại không chính xác.");
        }

        user.SetPassword(passwordHasher.HashPassword(request.NewPassword));
        user.RevokeAllRefreshTokens(); // Force logout on all devices after password change

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

