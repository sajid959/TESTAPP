using DSAGrind.Models.Entities;

namespace DSAGrind.Auth.API.Repositories;

public class MockUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private readonly ILogger<MockUserRepository> _logger;

    public MockUserRepository(ILogger<MockUserRepository> logger)
    {
        _logger = logger;
        
        // Add some mock data for demonstration
        _users.Add(new User
        {
            Id = "mock-user-1",
            Email = "admin@example.com",
            Username = "admin",
            FirstName = "Mock",
            LastName = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            Role = "Admin",
            IsEmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow.AddDays(-1)
        });
        
        _users.Add(new User
        {
            Id = "mock-user-2",
            Email = "user@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            Role = "User",
            IsEmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            UpdatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow.AddHours(-2)
        });
    }

    public Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        _logger.LogInformation("Mock GetByIdAsync for ID: {Id}, found: {Found}", id, user != null);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        _logger.LogInformation("Mock GetByEmailAsync for email: {Email}, found: {Found}", email, user != null);
        return Task.FromResult(user);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        _logger.LogInformation("Mock GetByUsernameAsync for username: {Username}, found: {Found}", username, user != null);
        return Task.FromResult(user);
    }

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        var exists = _users.Any(u => u.Id == id);
        _logger.LogInformation("Mock ExistsAsync for ID: {Id}, exists: {Exists}", id, exists);
        return Task.FromResult(exists);
    }

    public Task<User> CreateAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid().ToString();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        
        _users.Add(entity);
        _logger.LogInformation("Mock CreateAsync for user: {Email}", entity.Email);
        
        return Task.FromResult(entity);
    }

    public Task<User> UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        var existing = _users.FirstOrDefault(u => u.Id == entity.Id);
        if (existing != null)
        {
            var index = _users.IndexOf(existing);
            entity.UpdatedAt = DateTime.UtcNow;
            _users[index] = entity;
            _logger.LogInformation("Mock UpdateAsync for user: {Id}", entity.Id);
        }
        
        return Task.FromResult(entity);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _users.Remove(user);
            _logger.LogInformation("Mock DeleteAsync for user: {Id}", id);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    public Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((long)_users.Count);
    }

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.AsEnumerable());
    }

    public Task<User?> GetByGitHubIdAsync(string githubId, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.GitHubId == githubId);
        return Task.FromResult(user);
    }

    public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.GoogleId == googleId);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.EmailVerificationToken == token);
        return Task.FromResult(user);
    }

    public Task<User?> GetByResetPasswordTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.ResetPasswordToken == token);
        return Task.FromResult(user);
    }

    public Task UpdateLastLoginAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
        }
        
        return Task.CompletedTask;
    }

    public Task ClearEmailVerificationTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.EmailVerificationToken = null;
            user.IsEmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
        }
        
        return Task.CompletedTask;
    }

    public Task ClearResetPasswordTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.ResetPasswordToken = null;
            user.UpdatedAt = DateTime.UtcNow;
        }
        
        return Task.CompletedTask;
    }
}