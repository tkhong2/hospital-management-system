using FluentAssertions;
using HospitalMS.Identity.Application.Common.Interfaces;
using HospitalMS.Identity.Application.Features.Auth.Commands.Login;
using HospitalMS.Identity.Domain.Entities;
using HospitalMS.Identity.Domain.Enums;
using HospitalMS.Identity.Tests.Common;
using Moq;
using Xunit;

namespace HospitalMS.Identity.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    public LoginCommandHandlerTests()
    {
        _passwordHasherMock
            .Setup(p => p.VerifyPassword("CorrectPassword123!", "hashed_secret"))
            .Returns(true);

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<IReadOnlyList<string>>()))
            .Returns(("valid.jwt.token", DateTime.UtcNow.AddHours(1)));

        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns(("refresh_token_abc_123", DateTime.UtcNow.AddDays(7)));
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSuccessfulLoginResponseWithTokens()
    {
        // Arrange
        using var dbContext = IdentityTestDbContextFactory.Create();
        var user = new User
        {
            Username = "doctor_an",
            Email = "an.nguyen@hospital.vn",
            FullName = "Bs. Nguyễn Văn An",
            PasswordHash = "hashed_secret",
            Status = UserStatus.Active
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new LoginCommandHandler(dbContext, _passwordHasherMock.Object, _tokenServiceMock.Object);
        var command = new LoginCommand("doctor_an", "CorrectPassword123!", "127.0.0.1");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("valid.jwt.token");
        result.Value.RefreshToken.Should().Be("refresh_token_abc_123");
        result.Value.User.Username.Should().Be("doctor_an");
        result.Value.User.FullName.Should().Be("Bs. Nguyễn Văn An");

        // Refresh token persisted
        var savedUser = await dbContext.Users.FindAsync(user.Id);
        savedUser!.RefreshTokens.Should().ContainSingle();
        savedUser.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithInvalidUsername_ReturnsFailure()
    {
        // Arrange
        using var dbContext = IdentityTestDbContextFactory.Create();
        var handler = new LoginCommandHandler(dbContext, _passwordHasherMock.Object, _tokenServiceMock.Object);
        var command = new LoginCommand("nonexistent_user", "SomePassword123!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tên đăng nhập hoặc mật khẩu không chính xác");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_IncrementsAccessFailedCountAndReturnsRemainingAttempts()
    {
        // Arrange
        using var dbContext = IdentityTestDbContextFactory.Create();
        var user = new User
        {
            Username = "nurse_hoa",
            Email = "hoa.tran@hospital.vn",
            FullName = "Đd. Trần Thị Hoa",
            PasswordHash = "hashed_secret",
            Status = UserStatus.Active
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new LoginCommandHandler(dbContext, _passwordHasherMock.Object, _tokenServiceMock.Object);
        var command = new LoginCommand("nurse_hoa", "WrongPassword!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Bạn còn 4 lần thử");

        var updatedUser = await dbContext.Users.FindAsync(user.Id);
        updatedUser!.AccessFailedCount.Should().Be(1);
        updatedUser.IsLockedOut.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_With5ConsecutiveFailedAttempts_LocksOutUser()
    {
        // Arrange
        using var dbContext = IdentityTestDbContextFactory.Create();
        var user = new User
        {
            Username = "locked_user",
            Email = "locked@hospital.vn",
            FullName = "User Sắp Bị Khóa",
            PasswordHash = "hashed_secret",
            AccessFailedCount = 4, // 4 failed attempts previously
            Status = UserStatus.Active
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new LoginCommandHandler(dbContext, _passwordHasherMock.Object, _tokenServiceMock.Object);
        var command = new LoginCommand("locked_user", "WrongPassword!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tài khoản đã bị tạm khóa 15 phút");

        var updatedUser = await dbContext.Users.FindAsync(user.Id);
        updatedUser!.AccessFailedCount.Should().Be(5);
        updatedUser.IsLockedOut.Should().BeTrue();
        updatedUser.LockoutEnd.Should().NotBeNull();
        updatedUser.LockoutEnd!.Value.Should().BeAfter(DateTime.UtcNow.AddMinutes(14));
    }

    [Fact]
    public async Task Handle_WhenUserIsAlreadyLockedOut_RejectsLoginImmediately()
    {
        // Arrange
        using var dbContext = IdentityTestDbContextFactory.Create();
        var user = new User
        {
            Username = "already_locked",
            Email = "already_locked@hospital.vn",
            FullName = "User Đang Bị Khóa",
            PasswordHash = "hashed_secret",
            AccessFailedCount = 5,
            LockoutEnd = DateTime.UtcNow.AddMinutes(10),
            Status = UserStatus.Active
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new LoginCommandHandler(dbContext, _passwordHasherMock.Object, _tokenServiceMock.Object);
        var command = new LoginCommand("already_locked", "CorrectPassword123!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Tài khoản tạm thời bị khóa do đăng nhập sai nhiều lần");
    }
}

