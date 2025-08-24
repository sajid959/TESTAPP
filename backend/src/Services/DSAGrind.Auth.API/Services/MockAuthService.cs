using DSAGrind.Models.DTOs;

namespace DSAGrind.Auth.API.Services;

public class MockAuthService : IAuthService
{
    private readonly ILogger<MockAuthService> _logger;

    public MockAuthService(ILogger<MockAuthService> logger)
    {
        _logger = logger;
    }

    public Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress)
    {
        _logger.LogInformation("Mock login for email: {Email}", request.Email);
        
        return Task.FromResult(new AuthResponseDto
        {
            UserId = "mock-user-id",
            Username = request.Email,
            Email = request.Email,
            Token = "mock-jwt-token",
            RefreshToken = "mock-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    public Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress)
    {
        _logger.LogInformation("Mock register for email: {Email}", request.Email);
        
        return Task.FromResult(new AuthResponseDto
        {
            UserId = "mock-new-user-id",
            Username = request.Username,
            Email = request.Email,
            Token = "mock-jwt-token",
            RefreshToken = "mock-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        _logger.LogInformation("Mock refresh token");
        
        return Task.FromResult(new AuthResponseDto
        {
            UserId = "mock-user-id",
            Username = "mock-user",
            Email = "mock@example.com",
            Token = "mock-new-jwt-token",
            RefreshToken = "mock-new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    public Task<bool> RevokeTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Mock revoke token");
        return Task.FromResult(true);
    }

    public Task<UserProfileDto> GetProfileAsync(string userId)
    {
        _logger.LogInformation("Mock get profile for user: {UserId}", userId);
        
        return Task.FromResult(new UserProfileDto
        {
            UserId = userId,
            Username = "mock-user",
            Email = "mock@example.com",
            FirstName = "Mock",
            LastName = "User",
            Bio = "This is a mock user profile for development",
            Location = "Mock City",
            Website = "https://mock-website.com",
            GitHubUsername = "mockuser",
            LinkedInProfile = "https://linkedin.com/in/mockuser"
        });
    }

    public Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileRequestDto request)
    {
        _logger.LogInformation("Mock update profile for user: {UserId}", userId);
        
        return Task.FromResult(new UserProfileDto
        {
            UserId = userId,
            Username = request.Username ?? "mock-user",
            Email = "mock@example.com",
            FirstName = request.FirstName,
            LastName = request.LastName,
            Bio = request.Bio,
            Location = request.Location,
            Website = request.Website,
            GitHubUsername = request.GitHubUsername,
            LinkedInProfile = request.LinkedInProfile
        });
    }

    public Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
    {
        _logger.LogInformation("Mock change password for user: {UserId}", userId);
        return Task.FromResult(true);
    }

    public Task<bool> VerifyEmailAsync(string token)
    {
        _logger.LogInformation("Mock verify email with token: {Token}", token);
        return Task.FromResult(true);
    }

    public Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        _logger.LogInformation("Mock forgot password for email: {Email}", request.Email);
        return Task.FromResult(true);
    }

    public Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        _logger.LogInformation("Mock reset password");
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAccountAsync(string userId)
    {
        _logger.LogInformation("Mock delete account for user: {UserId}", userId);
        return Task.FromResult(true);
    }
}