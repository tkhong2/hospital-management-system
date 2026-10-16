using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Domain.Enums;

namespace HospitalMS.Identity.Domain.Entities;

public class User : AggregateRoot
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    // Security & Lockout properties
    public int AccessFailedCount { get; set; }
    public DateTime? LockoutEnd { get; set; }

    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    public List<UserRole> UserRoles { get; set; } = [];
    public List<RefreshToken> RefreshTokens { get; set; } = [];

    public void RecordLoginSuccess()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
    }

    public void RecordFailedLogin(int maxFailedAttempts = 5, int lockoutMinutes = 15)
    {
        AccessFailedCount++;
        if (AccessFailedCount >= maxFailedAttempts)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }
    }

    public void Unlock()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
    }

    public void SetPassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public RefreshToken AddRefreshToken(string token, DateTime expiresAt, string? ipAddress = null)
    {
        var refreshToken = new RefreshToken
        {
            UserId = Id,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
        RefreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void RevokeAllRefreshTokens(string? ipAddress = null)
    {
        foreach (var token in RefreshTokens.Where(t => t.IsActive))
        {
            token.Revoke(ipAddress);
        }
    }
}
