namespace DSAGrind.Common.Configuration;

public class OAuthSettings
{
    public const string SectionName = "OAuthSettings";

    public GoogleOAuthSettings Google { get; set; } = new();
    public GitHubOAuthSettings GitHub { get; set; } = new();
}

public class GoogleOAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "http://localhost:8080/api/oauth/google/callback";
    public string Scope { get; set; } = "openid profile email";
}

public class GitHubOAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "http://localhost:8080/api/oauth/github/callback";
    public string Scope { get; set; } = "user:email";
}