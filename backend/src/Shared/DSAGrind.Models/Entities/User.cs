using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DSAGrind.Models.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string? PasswordHash { get; set; }

    [BsonElement("firstName")]
    public string? FirstName { get; set; }

    [BsonElement("lastName")]
    public string? LastName { get; set; }

    [BsonElement("avatar")]
    public string? Avatar { get; set; }

    [BsonElement("role")]
    public string Role { get; set; } = "user"; // user, admin

    [BsonElement("isEmailVerified")]
    public bool IsEmailVerified { get; set; } = false;

    [BsonElement("emailVerificationToken")]
    public string? EmailVerificationToken { get; set; }

    [BsonElement("resetPasswordToken")]
    public string? ResetPasswordToken { get; set; }

    [BsonElement("resetPasswordExpires")]
    public DateTime? ResetPasswordExpires { get; set; }

    [BsonElement("githubId")]
    public string? GitHubId { get; set; }

    [BsonElement("googleId")]
    public string? GoogleId { get; set; }

    [BsonElement("refreshTokens")]
    public List<RefreshToken> RefreshTokens { get; set; } = new();

    [BsonElement("subscriptionPlan")]
    public string SubscriptionPlan { get; set; } = "free"; // free, premium

    [BsonElement("subscriptionStatus")]
    public string SubscriptionStatus { get; set; } = "active";

    [BsonElement("subscriptionExpires")]
    public DateTime? SubscriptionExpires { get; set; }

    [BsonElement("totalSolved")]
    public int TotalSolved { get; set; } = 0;

    [BsonElement("rank")]
    public int Rank { get; set; } = 0;

    [BsonElement("profile")]
    public UserProfile Profile { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class RefreshToken
{
    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("expires")]
    public DateTime Expires { get; set; }

    [BsonElement("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    [BsonElement("createdByIp")]
    public string CreatedByIp { get; set; } = string.Empty;

    [BsonElement("revoked")]
    public DateTime? Revoked { get; set; }

    [BsonElement("revokedByIp")]
    public string? RevokedByIp { get; set; }

    [BsonElement("replacedByToken")]
    public string? ReplacedByToken { get; set; }

    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsRevoked => Revoked != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}

public class UserProfile
{
    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("website")]
    public string? Website { get; set; }

    [BsonElement("company")]
    public string? Company { get; set; }

    [BsonElement("skills")]
    public List<string> Skills { get; set; } = new();

    [BsonElement("preferences")]
    public UserPreferences Preferences { get; set; } = new();
}

public class UserPreferences
{
    [BsonElement("theme")]
    public string Theme { get; set; } = "system"; // light, dark, system

    [BsonElement("language")]
    public string Language { get; set; } = "en";

    [BsonElement("notifications")]
    public NotificationSettings Notifications { get; set; } = new();
}

public class NotificationSettings
{
    [BsonElement("email")]
    public bool Email { get; set; } = true;

    [BsonElement("push")]
    public bool Push { get; set; } = true;

    [BsonElement("contests")]
    public bool Contests { get; set; } = true;

    [BsonElement("submissions")]
    public bool Submissions { get; set; } = true;
}