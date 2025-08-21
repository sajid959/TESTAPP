using System.Security.Cryptography;
using AutoMapper;
using DSAGrind.Auth.API.Repositories;
using DSAGrind.Auth.API.Services;
using DSAGrind.Common.Services;
using DSAGrind.Events;
using DSAGrind.Models.DTOs;
using DSAGrind.Models.Entities;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace DSAGrind.Auth.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IOAuthService _oauthService;
    private readonly IEmailService _emailService;
    private readonly IKafkaService _kafkaService;
    private readonly IRedisService _redisService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IOAuthService oauthService,
        IEmailService emailService,
        IKafkaService kafkaService,
        IRedisService redisService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _oauthService = oauthService;
        _emailService = emailService;
        _kafkaService = kafkaService;
        _redisService = redisService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
    {
        // Check rate limiting
        var rateLimitKey = $"login_attempts:{request.Email}";
        var isRateLimited = await _redisService.IsRateLimitedAsync(rateLimitKey, 5, TimeSpan.FromMinutes(15), cancellationToken);
        if (isRateLimited)
        {
            throw new UnauthorizedAccessException("Too many login attempts. Please try again later.");
        }

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await _redisService.IncrementAsync(rateLimitKey, cancellationToken: cancellationToken);
            await _redisService.ExpireAsync(rateLimitKey, TimeSpan.FromMinutes(15), cancellationToken);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Clear rate limiting on successful login
        await _redisService.DeleteAsync(rateLimitKey, cancellationToken);

        if (!user.IsEmailVerified)
        {
            throw new UnauthorizedAccessException("Email not verified. Please check your email for verification instructions.");
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Add refresh token to user
        var newRefreshToken = new RefreshToken
        {
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        user.RefreshTokens.Add(newRefreshToken);

        // Remove old refresh tokens (keep only the latest 5)
        user.RefreshTokens = user.RefreshTokens
            .Where(rt => rt.IsActive)
            .OrderByDescending(rt => rt.Created)
            .Take(5)
            .ToList();

        await _userRepository.UpdateRefreshTokensAsync(user.Id, user.RefreshTokens, cancellationToken);
        await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow, cancellationToken);

        // Publish login event
        await _kafkaService.PublishAsync("user-events", new UserLoginEvent
        {
            UserId = user.Id,
            Username = user.Username,
            IpAddress = ipAddress,
            UserAgent = string.Empty, // Will be set by controller
            LoginMethod = "password"
        }, cancellationToken: cancellationToken);

        // Cache user data
        await _redisService.SetAsync($"user:{user.Id}", _mapper.Map<UserDto>(user), TimeSpan.FromMinutes(30), cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserDto>(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
    {
        // Check if email or username already exists
        if (await _userRepository.IsEmailTakenAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        if (await _userRepository.IsUsernameTakenAsync(request.Username, cancellationToken))
        {
            throw new InvalidOperationException("Username is already taken.");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Generate email verification token
        var verificationToken = GenerateSecureToken();

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailVerificationToken = verificationToken,
            Role = "user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, cancellationToken);

        // Send verification email
        await _emailService.SendEmailVerificationAsync(user.Email, user.Username, verificationToken, cancellationToken);

        // Publish registration event
        await _kafkaService.PublishAsync("user-events", new UserRegisteredEvent
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        }, cancellationToken: cancellationToken);

        _logger.LogInformation("User registered successfully: {Email}", user.Email);

        // Generate tokens for immediate login (but mark as unverified)
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        user.RefreshTokens.Add(newRefreshToken);
        await _userRepository.UpdateRefreshTokensAsync(user.Id, user.RefreshTokens, cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserDto>(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var token = user.RefreshTokens.Single(rt => rt.Token == refreshToken);

        if (!token.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        // Replace old refresh token with new one
        var newRefreshToken = GenerateRefreshToken();
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReplacedByToken = newRefreshToken;

        var newToken = new RefreshToken
        {
            Token = newRefreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        user.RefreshTokens.Add(newToken);
        await _userRepository.UpdateRefreshTokensAsync(user.Id, user.RefreshTokens, cancellationToken);

        // Generate new access token
        var accessToken = _jwtService.GenerateAccessToken(user);

        // Update cache
        await _redisService.SetAsync($"user:{user.Id}", _mapper.Map<UserDto>(user), TimeSpan.FromMinutes(30), cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            User = _mapper.Map<UserDto>(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));

        if (user == null)
        {
            return false;
        }

        var success = await _userRepository.RevokeRefreshTokenAsync(user.Id, refreshToken, ipAddress, cancellationToken: cancellationToken);

        if (success)
        {
            await _kafkaService.PublishAsync("user-events", new RefreshTokenRevokedEvent
            {
                UserId = user.Id,
                RefreshToken = refreshToken,
                RevokedByIp = ipAddress
            }, cancellationToken: cancellationToken);

            // Clear user cache
            await _redisService.DeleteAsync($"user:{user.Id}", cancellationToken);
        }

        return success;
    }

    public async Task<bool> RevokeAllTokensAsync(string userId, string ipAddress, CancellationToken cancellationToken = default)
    {
        var success = await _userRepository.RevokeAllRefreshTokensAsync(userId, ipAddress, cancellationToken);

        if (success)
        {
            // Clear user cache
            await _redisService.DeleteAsync($"user:{userId}", cancellationToken);
        }

        return success;
    }

    public async Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailVerificationTokenAsync(token, cancellationToken);
        if (user == null)
        {
            return false;
        }

        var success = await _userRepository.UpdateEmailVerificationAsync(user.Id, true, null, cancellationToken);

        if (success)
        {
            await _kafkaService.PublishAsync("user-events", new UserEmailVerifiedEvent
            {
                UserId = user.Id,
                Email = user.Email
            }, cancellationToken: cancellationToken);

            await _emailService.SendWelcomeEmailAsync(user.Email, user.Username, cancellationToken);

            // Clear user cache to force refresh
            await _redisService.DeleteAsync($"user:{user.Id}", cancellationToken);
        }

        return success;
    }

    public async Task<bool> ResendEmailVerificationAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null || user.IsEmailVerified)
        {
            return false;
        }

        var verificationToken = GenerateSecureToken();
        await _userRepository.UpdateEmailVerificationAsync(user.Id, false, verificationToken, cancellationToken);
        await _emailService.SendEmailVerificationAsync(user.Email, user.Username, verificationToken, cancellationToken);

        return true;
    }

    public async Task<bool> ForgotPasswordAsync(string email, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            // Don't reveal that email doesn't exist
            return true;
        }

        var resetToken = GenerateSecureToken();
        var expires = DateTime.UtcNow.AddHours(1);

        await _userRepository.UpdatePasswordResetTokenAsync(user.Id, resetToken, expires, cancellationToken);
        await _emailService.SendPasswordResetAsync(user.Email, user.Username, resetToken, cancellationToken);

        await _kafkaService.PublishAsync("user-events", new PasswordResetRequestedEvent
        {
            UserId = user.Id,
            Email = user.Email,
            ResetToken = resetToken,
            IpAddress = ipAddress
        }, cancellationToken: cancellationToken);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByResetPasswordTokenAsync(request.Token, cancellationToken);
        if (user == null)
        {
            return false;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        var success = await _userRepository.UpdatePasswordAsync(user.Id, passwordHash, cancellationToken);

        if (success)
        {
            // Revoke all refresh tokens for security
            await _userRepository.RevokeAllRefreshTokensAsync(user.Id, ipAddress, cancellationToken);

            await _kafkaService.PublishAsync("user-events", new PasswordResetCompletedEvent
            {
                UserId = user.Id,
                Email = user.Email,
                IpAddress = ipAddress
            }, cancellationToken: cancellationToken);

            // Clear user cache
            await _redisService.DeleteAsync($"user:{user.Id}", cancellationToken);
        }

        return success;
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        var success = await _userRepository.UpdatePasswordAsync(user.Id, passwordHash, cancellationToken);

        if (success)
        {
            // Clear user cache
            await _redisService.DeleteAsync($"user:{userId}", cancellationToken);
        }

        return success;
    }

    public async Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Try cache first
        var cachedUser = await _redisService.GetAsync<UserDto>($"user:{userId}", cancellationToken);
        if (cachedUser != null)
        {
            return cachedUser;
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return null;
        }

        var userDto = _mapper.Map<UserDto>(user);

        // Cache for 30 minutes
        await _redisService.SetAsync($"user:{userId}", userDto, TimeSpan.FromMinutes(30), cancellationToken);

        return userDto;
    }

    public async Task<UserDto?> UpdateProfileAsync(string userId, UserProfileDto profile, CancellationToken cancellationToken = default)
    {
        var userProfile = _mapper.Map<UserProfile>(profile);
        var success = await _userRepository.UpdateProfileAsync(userId, userProfile, cancellationToken);

        if (!success)
        {
            return null;
        }

        // Clear cache to force refresh
        await _redisService.DeleteAsync($"user:{userId}", cancellationToken);

        return await GetUserAsync(userId, cancellationToken);
    }

    public async Task<AuthResponseDto> OAuthLoginAsync(string provider, string code, string state, string ipAddress, CancellationToken cancellationToken = default)
    {
        var oauthUser = await _oauthService.ExchangeCodeForUserAsync(provider, code, state, cancellationToken);
        if (oauthUser == null)
        {
            throw new UnauthorizedAccessException("OAuth authentication failed.");
        }

        User? user = provider.ToLower() switch
        {
            "google" => await _userRepository.GetByGoogleIdAsync(oauthUser.Id, cancellationToken),
            "github" => await _userRepository.GetByGitHubIdAsync(oauthUser.Id, cancellationToken),
            _ => null
        };

        if (user == null)
        {
            // Check if user exists with same email
            user = await _userRepository.GetByEmailAsync(oauthUser.Email, cancellationToken);

            if (user != null)
            {
                // Link OAuth account to existing user
                switch (provider.ToLower())
                {
                    case "google":
                        user.GoogleId = oauthUser.Id;
                        break;
                    case "github":
                        user.GitHubId = oauthUser.Id;
                        break;
                }

                user.IsEmailVerified = true; // OAuth emails are pre-verified
                await _userRepository.UpdateAsync(user.Id, user, cancellationToken);
            }
            else
            {
                // Create new user
                user = new User
                {
                    Username = oauthUser.Username ?? oauthUser.Email.Split('@')[0],
                    Email = oauthUser.Email,
                    FirstName = oauthUser.FirstName,
                    LastName = oauthUser.LastName,
                    Avatar = oauthUser.Avatar,
                    IsEmailVerified = true,
                    Role = "user",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                switch (provider.ToLower())
                {
                    case "google":
                        user.GoogleId = oauthUser.Id;
                        break;
                    case "github":
                        user.GitHubId = oauthUser.Id;
                        break;
                }

                // Ensure username is unique
                var baseUsername = user.Username;
                int counter = 1;
                while (await _userRepository.IsUsernameTakenAsync(user.Username, cancellationToken))
                {
                    user.Username = $"{baseUsername}{counter}";
                    counter++;
                }

                await _userRepository.CreateAsync(user, cancellationToken);

                await _kafkaService.PublishAsync("user-events", new UserRegisteredEvent
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                }, cancellationToken: cancellationToken);
            }
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        user.RefreshTokens.Add(newRefreshToken);
        await _userRepository.UpdateRefreshTokensAsync(user.Id, user.RefreshTokens, cancellationToken);
        await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow, cancellationToken);

        await _kafkaService.PublishAsync("user-events", new UserLoginEvent
        {
            UserId = user.Id,
            Username = user.Username,
            IpAddress = ipAddress,
            LoginMethod = provider
        }, cancellationToken: cancellationToken);

        // Cache user data
        await _redisService.SetAsync($"user:{user.Id}", _mapper.Map<UserDto>(user), TimeSpan.FromMinutes(30), cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserDto>(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
    }

    public async Task<string> GenerateOAuthUrlAsync(string provider, string state, CancellationToken cancellationToken = default)
    {
        return await _oauthService.GenerateAuthorizationUrlAsync(provider, state, cancellationToken);
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return _jwtService.ValidateToken(token);
    }

    public async Task<string?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return _jwtService.GetUserIdFromToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static string GenerateSecureToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}