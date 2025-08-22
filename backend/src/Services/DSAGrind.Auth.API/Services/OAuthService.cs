using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using DSAGrind.Common.Configuration;
using DSAGrind.Common.Services;

namespace DSAGrind.Auth.API.Services;

public class OAuthService : IOAuthService
{
    private readonly OAuthSettings _oauthSettings;
    private readonly IRedisService _redisService;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(
        IOptions<OAuthSettings> oauthSettings,
        IRedisService redisService,
        HttpClient httpClient,
        ILogger<OAuthService> logger)
    {
        _oauthSettings = oauthSettings.Value;
        _redisService = redisService;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GenerateAuthorizationUrlAsync(string provider, string state, CancellationToken cancellationToken = default)
    {
        // Store state in Redis for validation (expires in 10 minutes)
        await _redisService.SetAsync($"oauth_state:{state}", provider, TimeSpan.FromMinutes(10), cancellationToken);

        return provider.ToLower() switch
        {
            "google" => GenerateGoogleAuthUrl(state),
            "github" => GenerateGitHubAuthUrl(state),
            _ => throw new ArgumentException($"Unsupported OAuth provider: {provider}")
        };
    }

    public async Task<OAuthUser?> ExchangeCodeForUserAsync(string provider, string code, string state, CancellationToken cancellationToken = default)
    {
        // Validate state
        var storedProvider = await _redisService.GetAsync<string>($"oauth_state:{state}", cancellationToken);
        if (string.IsNullOrEmpty(storedProvider))
        {
            _logger.LogWarning("Invalid OAuth state: {State}", state);
            return null;
        }

        // Clean up state after validation
        await _redisService.DeleteAsync($"oauth_state:{state}", cancellationToken);

        return provider.ToLower() switch
        {
            "google" => await ExchangeGoogleCodeAsync(code, cancellationToken),
            "github" => await ExchangeGitHubCodeAsync(code, cancellationToken),
            _ => null
        };
    }

    public async Task<OAuthUser?> GetGoogleUserAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var userResponse = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo", cancellationToken);
            
            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get Google user info: {StatusCode}", userResponse.StatusCode);
                return null;
            }

            var userJson = await userResponse.Content.ReadAsStringAsync(cancellationToken);
            using var userDoc = JsonDocument.Parse(userJson);
            var root = userDoc.RootElement;

            return new OAuthUser
            {
                Id = root.GetProperty("id").GetString() ?? string.Empty,
                Email = root.GetProperty("email").GetString() ?? string.Empty,
                FirstName = root.TryGetProperty("given_name", out var givenName) ? givenName.GetString() : null,
                LastName = root.TryGetProperty("family_name", out var familyName) ? familyName.GetString() : null,
                Avatar = root.TryGetProperty("picture", out var picture) ? picture.GetString() : null,
                RawData = JsonSerializer.Deserialize<Dictionary<string, object>>(userJson) ?? new()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google user info");
            return null;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<OAuthUser?> GetGitHubUserAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("token", accessToken);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DSAGrind/1.0");

            var userResponse = await _httpClient.GetAsync("https://api.github.com/user", cancellationToken);
            
            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get GitHub user info: {StatusCode}", userResponse.StatusCode);
                return null;
            }

            var userJson = await userResponse.Content.ReadAsStringAsync(cancellationToken);
            using var userDoc = JsonDocument.Parse(userJson);
            var root = userDoc.RootElement;

            // Get primary email (GitHub might not provide email in user endpoint)
            var email = root.TryGetProperty("email", out var emailProp) && !emailProp.ValueKind.Equals(JsonValueKind.Null) 
                ? emailProp.GetString() 
                : await GetGitHubPrimaryEmailAsync(accessToken, cancellationToken);

            var fullName = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
            var nameParts = fullName?.Split(' ', 2);

            return new OAuthUser
            {
                Id = root.GetProperty("id").GetInt32().ToString(),
                Email = email ?? string.Empty,
                Username = root.TryGetProperty("login", out var login) ? login.GetString() : null,
                FirstName = nameParts?.Length > 0 ? nameParts[0] : null,
                LastName = nameParts?.Length > 1 ? nameParts[1] : null,
                Avatar = root.TryGetProperty("avatar_url", out var avatar) ? avatar.GetString() : null,
                RawData = JsonSerializer.Deserialize<Dictionary<string, object>>(userJson) ?? new()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitHub user info");
            return null;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
        }
    }

    private string GenerateGoogleAuthUrl(string state)
    {
        var googleSettings = _oauthSettings.Google;
        var queryParams = new StringBuilder();
        queryParams.Append($"response_type=code");
        queryParams.Append($"&client_id={googleSettings.ClientId}");
        queryParams.Append($"&redirect_uri={Uri.EscapeDataString(googleSettings.RedirectUri)}");
        queryParams.Append($"&scope={Uri.EscapeDataString(googleSettings.Scope)}");
        queryParams.Append($"&state={state}");
        queryParams.Append("&access_type=offline");
        queryParams.Append("&include_granted_scopes=true");

        return $"https://accounts.google.com/o/oauth2/v2/auth?{queryParams}";
    }

    private string GenerateGitHubAuthUrl(string state)
    {
        var githubSettings = _oauthSettings.GitHub;
        var queryParams = new StringBuilder();
        queryParams.Append($"response_type=code");
        queryParams.Append($"&client_id={githubSettings.ClientId}");
        queryParams.Append($"&redirect_uri={Uri.EscapeDataString(githubSettings.RedirectUri)}");
        queryParams.Append($"&scope={Uri.EscapeDataString(githubSettings.Scope)}");
        queryParams.Append($"&state={state}");

        return $"https://github.com/login/oauth/authorize?{queryParams}";
    }

    private async Task<OAuthUser?> ExchangeGoogleCodeAsync(string code, CancellationToken cancellationToken)
    {
        try
        {
            var googleSettings = _oauthSettings.Google;

            // Exchange code for access token
            var tokenRequest = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", googleSettings.ClientId },
                { "client_secret", googleSettings.ClientSecret },
                { "redirect_uri", googleSettings.RedirectUri },
                { "grant_type", "authorization_code" }
            };

            var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", 
                new FormUrlEncodedContent(tokenRequest), cancellationToken);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to exchange Google code for token: {StatusCode}", tokenResponse.StatusCode);
                return null;
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            using var tokenDoc = JsonDocument.Parse(tokenJson);
            var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token received from Google");
                return null;
            }

            return await GetGoogleUserAsync(accessToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging Google OAuth code");
            return null;
        }
    }

    private async Task<OAuthUser?> ExchangeGitHubCodeAsync(string code, CancellationToken cancellationToken)
    {
        try
        {
            var githubSettings = _oauthSettings.GitHub;

            // Exchange code for access token
            var tokenRequest = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", githubSettings.ClientId },
                { "client_secret", githubSettings.ClientSecret },
                { "redirect_uri", githubSettings.RedirectUri }
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var tokenResponse = await _httpClient.PostAsync("https://github.com/login/oauth/access_token", 
                new FormUrlEncodedContent(tokenRequest), cancellationToken);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to exchange GitHub code for token: {StatusCode}", tokenResponse.StatusCode);
                return null;
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            using var tokenDoc = JsonDocument.Parse(tokenJson);
            var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token received from GitHub");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            return await GetGitHubUserAsync(accessToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging GitHub OAuth code");
            return null;
        }
    }

    private async Task<string?> GetGitHubPrimaryEmailAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("token", accessToken);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DSAGrind/1.0");

            var emailResponse = await _httpClient.GetAsync("https://api.github.com/user/emails", cancellationToken);
            
            if (!emailResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var emailJson = await emailResponse.Content.ReadAsStringAsync(cancellationToken);
            using var emailDoc = JsonDocument.Parse(emailJson);
            
            foreach (var emailElement in emailDoc.RootElement.EnumerateArray())
            {
                if (emailElement.TryGetProperty("primary", out var isPrimary) && isPrimary.GetBoolean())
                {
                    return emailElement.GetProperty("email").GetString();
                }
            }

            // If no primary email found, return the first verified email
            foreach (var emailElement in emailDoc.RootElement.EnumerateArray())
            {
                if (emailElement.TryGetProperty("verified", out var isVerified) && isVerified.GetBoolean())
                {
                    return emailElement.GetProperty("email").GetString();
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitHub primary email");
            return null;
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
        }
    }
}