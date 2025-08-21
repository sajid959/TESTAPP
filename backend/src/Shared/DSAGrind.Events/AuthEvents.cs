namespace DSAGrind.Events;

public class UserRegisteredEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string? ReferralCode { get; set; }
}

public class UserEmailVerifiedEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;
}

public class UserLoginEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;
    public string LoginMethod { get; set; } = string.Empty; // password, oauth
}

public class UserLogoutEvent
{
    public string UserId { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime LogoutAt { get; set; } = DateTime.UtcNow;
}

public class PasswordResetRequestedEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
}

public class PasswordResetCompletedEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ResetAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
}

public class RefreshTokenRevokedEvent
{
    public string UserId { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string? ReplacedByToken { get; set; }
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
    public string RevokedByIp { get; set; } = string.Empty;
}