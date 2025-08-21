using System.Security.Claims;
using DSAGrind.Models.Entities;

namespace DSAGrind.Common.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromToken(string token);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateToken(string token);
    string? GetUserIdFromToken(string token);
    string? GetUsernameFromToken(string token);
    string? GetRoleFromToken(string token);
    DateTime GetTokenExpiration(string token);
    bool IsTokenExpired(string token);
}