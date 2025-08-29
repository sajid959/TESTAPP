using MongoDB.Driver;
using Microsoft.Extensions.Options;
using DSAGrind.Common.Repositories;
using DSAGrind.Common.Configuration;
using DSAGrind.Models.Entities;

namespace DSAGrind.Auth.API.Repositories;

public class UserRepository : MongoRepository<User>, IUserRepository
{
    public UserRepository(IMongoDatabase database) : base(database, "users")
    {
        // Create indexes for better performance
        CreateIndexesAsync().GetAwaiter().GetResult();
    }

    private async Task CreateIndexesAsync()
    {
        var emailIndex = Builders<User>.IndexKeys.Ascending(u => u.Email);
        var usernameIndex = Builders<User>.IndexKeys.Ascending(u => u.Username);
        var githubIdIndex = Builders<User>.IndexKeys.Ascending(u => u.GitHubId);
        var googleIdIndex = Builders<User>.IndexKeys.Ascending(u => u.GoogleId);
        var emailVerificationTokenIndex = Builders<User>.IndexKeys.Ascending(u => u.EmailVerificationToken);
        var resetPasswordTokenIndex = Builders<User>.IndexKeys.Ascending(u => u.ResetPasswordToken);

        await _collection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<User>(emailIndex, new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<User>(usernameIndex, new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<User>(githubIdIndex, new CreateIndexOptions { Sparse = true }),
            new CreateIndexModel<User>(googleIdIndex, new CreateIndexOptions { Sparse = true }),
            new CreateIndexModel<User>(emailVerificationTokenIndex, new CreateIndexOptions { Sparse = true }),
            new CreateIndexModel<User>(resetPasswordTokenIndex, new CreateIndexOptions { Sparse = true })
        });
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await GetAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await GetAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByGitHubIdAsync(string githubId, CancellationToken cancellationToken = default)
    {
        return await GetAsync(u => u.GitHubId == githubId, cancellationToken);
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        return await GetAsync(u => u.GoogleId == googleId, cancellationToken);
    }

    public async Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await GetAsync(u => u.EmailVerificationToken == token, cancellationToken);
    }

    public async Task<User?> GetByResetPasswordTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await GetAsync(u => u.ResetPasswordToken == token && u.ResetPasswordExpires > DateTime.UtcNow, cancellationToken);
    }

    public async Task<bool> IsEmailTakenAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExistsAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsUsernameTakenAsync(string username, CancellationToken cancellationToken = default)
    {
        return await ExistsAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> UpdateRefreshTokensAsync(string userId, List<RefreshToken> refreshTokens, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update
            .Set(u => u.RefreshTokens, refreshTokens)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string userId, string refreshToken, string revokedByIp, string? replacedByToken = null, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        var token = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
        if (token == null) return false;

        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = revokedByIp;
        token.ReplacedByToken = replacedByToken;

        return await UpdateRefreshTokensAsync(userId, user.RefreshTokens, cancellationToken);
    }

    public async Task<bool> RevokeAllRefreshTokensAsync(string userId, string revokedByIp, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        foreach (var token in user.RefreshTokens.Where(rt => rt.IsActive))
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = revokedByIp;
        }

        return await UpdateRefreshTokensAsync(userId, user.RefreshTokens, cancellationToken);
    }

    public async Task<List<RefreshToken>> GetActiveRefreshTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        return user?.RefreshTokens.Where(rt => rt.IsActive).ToList() ?? new List<RefreshToken>();
    }

    public async Task<bool> UpdateEmailVerificationAsync(string userId, bool isVerified, string? token = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update
            .Set(u => u.IsEmailVerified, isVerified)
            .Set(u => u.EmailVerificationToken, token)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdatePasswordResetTokenAsync(string userId, string? token, DateTime? expires, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update
            .Set(u => u.ResetPasswordToken, token)
            .Set(u => u.ResetPasswordExpires, expires)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdatePasswordAsync(string userId, string passwordHash, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update
            .Set(u => u.PasswordHash, passwordHash)
            .Set(u => u.ResetPasswordToken, null)
            .Set(u => u.ResetPasswordExpires, null)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateLastLoginAsync(string userId, DateTime lastLogin, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update
            .Set(u => u.UpdatedAt, lastLogin);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateProfileAsync(string userId, UserProfile profile, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update
            .Set(u => u.Profile, profile)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }
}