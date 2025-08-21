using DSAGrind.Models.DTOs;
using DSAGrind.Models.Entities;

namespace DSAGrind.Auth.API.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> RevokeAllTokensAsync(string userId, string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> ResendEmailVerificationAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ForgotPasswordAsync(string email, string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateProfileAsync(string userId, UserProfileDto profile, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> OAuthLoginAsync(string provider, string code, string state, string ipAddress, CancellationToken cancellationToken = default);
    Task<string> GenerateOAuthUrlAsync(string provider, string state, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<string?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default);
}