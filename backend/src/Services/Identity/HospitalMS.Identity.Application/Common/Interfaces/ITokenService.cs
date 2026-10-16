using System.Security.Claims;
using HospitalMS.Identity.Domain.Entities;

namespace HospitalMS.Identity.Application.Common.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions);

    (string Token, DateTime ExpiresAt) GenerateRefreshToken();

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

