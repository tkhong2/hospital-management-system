using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using MediatR;

namespace HospitalMS.Identity.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<Result<LoginResponseDto>>;

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
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> AccessibleApps,
    string? DepartmentName
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

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    public Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Demonstration authentication validation
        if (request.Username != "admin" && request.Username != "doctor")
        {
            return Task.FromResult(Result.Failure<LoginResponseDto>("Tên đăng nhập hoặc mật khẩu không chính xác."));
        }

        var roles = request.Username == "admin" ? new[] { "Admin", "Manager" } : new[] { "Doctor" };
        var apps = new[] { "healthexam", "outpatient", "inpatient", "pharmacy", "patient", "catalog", "admin" };

        var user = new UserInfoDto(
            Id: Guid.NewGuid(),
            Username: request.Username,
            FullName: request.Username == "admin" ? "Quản trị viên Hệ thống" : "Bác sĩ CKI - Trần Văn Bình",
            Email: $"{request.Username}@hospital.local",
            Roles: roles,
            Permissions: new[] { "*" },
            AccessibleApps: apps,
            DepartmentName: "Ban Giám đốc & CNTT"
        );

        var response = new LoginResponseDto(
            AccessToken: "demo-jwt-access-token-" + Guid.NewGuid(),
            RefreshToken: "demo-jwt-refresh-token-" + Guid.NewGuid(),
            ExpiresAt: DateTime.UtcNow.AddHours(2),
            User: user
        );

        return Task.FromResult(Result.Success(response));
    }
}
