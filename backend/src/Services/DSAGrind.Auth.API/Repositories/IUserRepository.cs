using DSAGrind.Common.Repositories;
using DSAGrind.Models.Entities;

namespace DSAGrind.Auth.API.Repositories;

public interface IUserRepository : IMongoRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByGitHubIdAsync(string githubId, CancellationToken cancellationToken = default);
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<User?> GetByResetPasswordTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> IsEmailTakenAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameTakenAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> UpdateRefreshTokensAsync(string userId, List<RefreshToken> refreshTokens, CancellationToken cancellationToken = default);
    Task<bool> RevokeRefreshTokenAsync(string userId, string refreshToken, string revokedByIp, string? replacedByToken = null, CancellationToken cancellationToken = default);
    Task<bool> RevokeAllRefreshTokensAsync(string userId, string revokedByIp, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetActiveRefreshTokensAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateEmailVerificationAsync(string userId, bool isVerified, string? token = null, CancellationToken cancellationToken = default);
    Task<bool> UpdatePasswordResetTokenAsync(string userId, string? token, DateTime? expires, CancellationToken cancellationToken = default);
    Task<bool> UpdatePasswordAsync(string userId, string passwordHash, CancellationToken cancellationToken = default);
    Task<bool> UpdateLastLoginAsync(string userId, DateTime lastLogin, CancellationToken cancellationToken = default);
    Task<bool> UpdateProfileAsync(string userId, UserProfile profile, CancellationToken cancellationToken = default);
}