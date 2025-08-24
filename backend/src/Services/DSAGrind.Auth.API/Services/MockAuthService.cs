using DSAGrind.Models.DTOs;

namespace DSAGrind.Auth.API.Services;

public class MockAuthService : IAuthService
{
    private readonly ILogger<MockAuthService> _logger;

    public MockAuthService(ILogger<MockAuthService> logger)
    {
        _logger = logger;
    }

    public Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock login for email: {Email}", request.Email);
        
        return Task.FromResult(new AuthResponseDto
        {
            AccessToken = "mock-jwt-token",
            RefreshToken = "mock-refresh-token",
            User = new UserDto
            {
                Id = "mock-user-id",
                Username = request.Email,
                Email = request.Email,
                Role = "User",
                IsEmailVerified = true,
                SubscriptionPlan = "Free",
                SubscriptionStatus = "Active",
                CreatedAt = DateTime.UtcNow
            },
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    public Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock register for email: {Email}", request.Email);
        
        return Task.FromResult(new AuthResponseDto
        {
            AccessToken = "mock-jwt-token",
            RefreshToken = "mock-refresh-token",
            User = new UserDto
            {
                Id = "mock-new-user-id",
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = "User",
                IsEmailVerified = false,
                SubscriptionPlan = "Free",
                SubscriptionStatus = "Active",
                CreatedAt = DateTime.UtcNow
            },
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock refresh token");
        
        return Task.FromResult(new AuthResponseDto
        {
            AccessToken = "mock-new-jwt-token",
            RefreshToken = "mock-new-refresh-token",
            User = new UserDto
            {
                Id = "mock-user-id",
                Username = "mock-user",
                Email = "mock@example.com",
                Role = "User",
                IsEmailVerified = true,
                SubscriptionPlan = "Free",
                SubscriptionStatus = "Active",
                CreatedAt = DateTime.UtcNow
            },
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    public Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock revoke token");
        return Task.FromResult(true);
    }

    public Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock get user for user: {UserId}", userId);
        
        return Task.FromResult<UserDto?>(new UserDto
        {
            Id = userId,
            Username = "mock-user",
            Email = "mock@example.com",
            FirstName = "Mock",
            LastName = "User",
            Role = "User",
            IsEmailVerified = true,
            SubscriptionPlan = "Free",
            SubscriptionStatus = "Active",
            CreatedAt = DateTime.UtcNow,
            Profile = new UserProfileDto
            {
                Bio = "This is a mock user profile for development",
                Location = "Mock City",
                Website = "https://mock-website.com"
            }
        });
    }

    public Task<UserDto?> UpdateProfileAsync(string userId, UserProfileDto profile, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock update profile for user: {UserId}", userId);
        
        return Task.FromResult<UserDto?>(new UserDto
        {
            Id = userId,
            Username = "mock-user",
            Email = "mock@example.com",
            FirstName = "Mock",
            LastName = "User",
            Role = "User",
            IsEmailVerified = true,
            SubscriptionPlan = "Free",
            SubscriptionStatus = "Active",
            CreatedAt = DateTime.UtcNow,
            Profile = profile
        });
    }

    public Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock change password for user: {UserId}", userId);
        return Task.FromResult(true);
    }

    public Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock verify email with token: {Token}", token);
        return Task.FromResult(true);
    }

    public Task<bool> ForgotPasswordAsync(string email, string ipAddress, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock forgot password for email: {Email}", email);
        return Task.FromResult(true);
    }

    public Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock reset password");
        return Task.FromResult(true);
    }

    public Task<bool> RevokeAllTokensAsync(string userId, string ipAddress, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock revoke all tokens for user: {UserId}", userId);
        return Task.FromResult(true);
    }

    public Task<bool> ResendEmailVerificationAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock resend email verification for email: {Email}", email);
        return Task.FromResult(true);
    }

    public Task<AuthResponseDto> OAuthLoginAsync(string provider, string code, string state, string ipAddress, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock OAuth login for provider: {Provider}", provider);
        
        return Task.FromResult(new AuthResponseDto
        {
            AccessToken = "mock-oauth-jwt-token",
            RefreshToken = "mock-oauth-refresh-token",
            User = new UserDto
            {
                Id = "mock-oauth-user-id",
                Username = "mock-oauth-user",
                Email = "oauth@example.com",
                Role = "User",
                IsEmailVerified = true,
                SubscriptionPlan = "Free",
                SubscriptionStatus = "Active",
                CreatedAt = DateTime.UtcNow
            },
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    public Task<string> GenerateOAuthUrlAsync(string provider, string state, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock generate OAuth URL for provider: {Provider}", provider);
        return Task.FromResult($"https://mock-oauth-{provider}.com/auth?state={state}");
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock validate token");
        return Task.FromResult(true);
    }

    public Task<string?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock get user ID from token");
        return Task.FromResult<string?>("mock-user-id");
    }
}