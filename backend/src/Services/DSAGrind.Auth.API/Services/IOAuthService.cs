namespace DSAGrind.Auth.API.Services;

public interface IOAuthService
{
    Task<string> GenerateAuthorizationUrlAsync(string provider, string state, CancellationToken cancellationToken = default);
    Task<OAuthUser?> ExchangeCodeForUserAsync(string provider, string code, string state, CancellationToken cancellationToken = default);
    Task<OAuthUser?> GetGoogleUserAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<OAuthUser?> GetGitHubUserAsync(string accessToken, CancellationToken cancellationToken = default);
}

public class OAuthUser
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Avatar { get; set; }
    public Dictionary<string, object> RawData { get; set; } = new();
}

public class OAuthTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string? Scope { get; set; }
}