using FluentAssertions;
using HospitalMS.Identity.Application.Common.Interfaces;
using HospitalMS.Identity.Application.Features.Auth.Commands.RefreshToken;
using HospitalMS.Identity.Domain.Entities;
using HospitalMS.Identity.Domain.Enums;
using HospitalMS.Identity.Tests.Common;
using Moq;
using Xunit;

namespace HospitalMS.Identity.Tests.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    public RefreshTokenCommandHandlerTests()
    {
        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<IReadOnlyList<string>>()))
            .Returns(("new.jwt.access.token", DateTime.UtcNow.AddHours(1)));

        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns(("new_rotated_refresh_token", DateTime.UtcNow.AddDays(7)));
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_RotatesTokensAndReturnsNewLoginResponse()
    {
        // Arrange
        using var dbContext = IdentityTestDbContextFactory.Create();
        var user = new User
        {
            Username = "pharmacist_mai",
            Email = "mai.le@hospital.vn",
            FullName = "Ds. Lê Thị Mai",
            PasswordHash = "hashed_secret",
            Status = UserStatus.Active
        };
        user.AddRefreshToken("initial_refresh_token", DateTime.UtcNow.AddDays(3), "192.168.1.10");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new RefreshTokenCommandHandler(dbContext, _tokenServiceMock.Object);
        var command = new RefreshTokenCommand("old_access_token", "initial_refresh_token", "192.168.1.10");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("new.jwt.access.token");
        result.Value.RefreshToken.Should().Be("new_rotated_refresh_token");

        // Old token must be revoked and new token added
        var tokens = user.RefreshTokens.ToList();
        tokens.Should().HaveCount(2);

        var oldToken = tokens.First(t => t.Token == "initial_refresh_token");
        oldToken.IsRevoked.Should().BeTrue();
        oldToken.ReplacedByToken.Should().Be("new_rotated_refresh_token");

        var newToken = tokens.First(t => t.Token == "new_rotated_refresh_token");
        newToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ReturnsFailure()
    {
        // Arrange
        using var dbContext = IdentityTestDbContextFactory.Create();
        var user = new User
        {
            Username = "user_expired",
            Email = "expired@hospital.vn",
            FullName = "User Hết Hạn",
            PasswordHash = "hashed_secret",
            Status = UserStatus.Active
        };
        // Expired yesterday
        user.AddRefreshToken("expired_token", DateTime.UtcNow.AddDays(-1), "127.0.0.1");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new RefreshTokenCommandHandler(dbContext, _tokenServiceMock.Object);
        var command = new RefreshTokenCommand("some_token", "expired_token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Phiên đăng nhập đã hết hạn");
    }

    [Fact]
    public async Task Handle_WithRevokedRefreshToken_RevokesAllTokensAndReturnsSecurityAlert()
    {
        // Arrange (Simulate Token Theft: an attacker tries to reuse an already-revoked refresh token)
        using var dbContext = IdentityTestDbContextFactory.Create();
        var user = new User
        {
            Username = "user_victim",
            Email = "victim@hospital.vn",
            FullName = "Nạn Nhân Bị Lộ Token",
            PasswordHash = "hashed_secret",
            Status = UserStatus.Active
        };
        user.AddRefreshToken("stolen_old_token", DateTime.UtcNow.AddDays(2), "127.0.0.1");
        var firstToken = user.RefreshTokens.First();
        firstToken.Revoke("127.0.0.1", "legitimate_active_token");

        // Add the legitimate active token
        user.AddRefreshToken("legitimate_active_token", DateTime.UtcNow.AddDays(7), "127.0.0.1");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new RefreshTokenCommandHandler(dbContext, _tokenServiceMock.Object);
        // Attacker uses stolen_old_token
        var command = new RefreshTokenCommand("expired_access", "stolen_old_token", "attacker_ip");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Phiên đăng nhập không an toàn hoặc đã bị thu hồi");

        // All refresh tokens for this user must now be revoked
        var allTokens = user.RefreshTokens.ToList();
        allTokens.Should().OnlyContain(t => t.IsRevoked);
    }
}

