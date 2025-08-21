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
        var settings = GetProviderSettings(provider);
        if (settings == null)
        {
            throw new NotSupportedException($"OAuth provider '{provider}' is not supported.");
        }

        // Store state in Redis for security validation
        await _redisService.SetAsync($"oauth_state:{state}", provider, TimeSpan.FromMinutes(10), cancellationToken);

        var scopes = string.Join(" ", GetProviderScopes(provider));
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = settings.ClientId,
            ["redirect_uri"] = settings.RedirectUri,
            ["scope"] = scopes,
            ["response_type"] = "code",
            ["state"] = state
        };

        var queryString = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{settings.AuthorizationEndpoint}?{queryString}";
    }

    public async Task<OAuthUser?> ExchangeCodeForUserAsync(string provider, string code, string state, CancellationToken cancellationToken = default)
    {
        // Validate state
        var storedProvider = await _redisService.GetAsync<string>($"oauth_state:{state}", cancellationToken);
        if (storedProvider != provider)
        {
            _logger.LogWarning("OAuth state validation failed for provider {Provider}", provider);
            return null;
        }

        // Clean up state
        await _redisService.DeleteAsync($"oauth_state:{state}", cancellationToken);

        var settings = GetProviderSettings(provider);
        if (settings == null)
        {
            return null;
        }

        try
        {
            // Exchange code for access token
            var tokenResponse = await ExchangeCodeForTokenAsync(settings, code, cancellationToken);
            if (tokenResponse == null)
            {
                return null;
            }

            // Get user information
            return provider.ToLower() switch
            {
                "google" => await GetGoogleUserAsync(tokenResponse.AccessToken, cancellationToken),
                "github" => await GetGitHubUserAsync(tokenResponse.AccessToken, cancellationToken),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth exchange for provider {Provider}", provider);
            return null;
        }
    }

    private async Task<OAuthTokenResponse?> ExchangeCodeForTokenAsync(dynamic settings, string code, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = settings.ClientId,
            ["client_secret"] = settings.ClientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = settings.RedirectUri
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(settings.TokenEndpoint, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OAuth token exchange failed with status {StatusCode}: {Content}", 
                response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
            return null;
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        try
        {
            var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            return new OAuthTokenResponse
            {
                AccessToken = tokenData.GetProperty("access_token").GetString() ?? string.Empty,
                RefreshToken = tokenData.TryGetProperty("refresh_token", out var refreshToken) ? refreshToken.GetString() : null,
                TokenType = tokenData.TryGetProperty("token_type", out var tokenType) ? tokenType.GetString() ?? "Bearer" : "Bearer",
                ExpiresIn = tokenData.TryGetProperty("expires_in", out var expiresIn) ? expiresIn.GetInt32() : 3600,
                Scope = tokenData.TryGetProperty("scope", out var scope) ? scope.GetString() : null
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OAuth token response: {Content}", responseContent);
            return null;
        }
    }

    public async Task<OAuthUser?> GetGoogleUserAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.GetAsync(_oauthSettings.Google.UserInfoEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google user info request failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var userData = JsonSerializer.Deserialize<JsonElement>(content);

            return new OAuthUser
            {
                Id = userData.GetProperty("id").GetString() ?? string.Empty,
                Email = userData.GetProperty("email").GetString() ?? string.Empty,
                FirstName = userData.TryGetProperty("given_name", out var firstName) ? firstName.GetString() : null,
                LastName = userData.TryGetProperty("family_name", out var lastName) ? lastName.GetString() : null,
                Avatar = userData.TryGetProperty("picture", out var picture) ? picture.GetString() : null,
                Username = userData.TryGetProperty("email", out var email) ? email.GetString()?.Split('@')[0] : null,
                RawData = JsonSerializer.Deserialize<Dictionary<string, object>>(content) ?? new()
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
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            // Get user info
            var userResponse = await _httpClient.GetAsync(_oauthSettings.GitHub.UserInfoEndpoint, cancellationToken);
            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogError("GitHub user info request failed with status {StatusCode}", userResponse.StatusCode);
                return null;
            }

            var userContent = await userResponse.Content.ReadAsStringAsync(cancellationToken);
            var userData = JsonSerializer.Deserialize<JsonElement>(userContent);

            // Get user emails
            var emailResponse = await _httpClient.GetAsync(_oauthSettings.GitHub.UserEmailEndpoint, cancellationToken);
            string? primaryEmail = null;

            if (emailResponse.IsSuccessStatusCode)
            {
                var emailContent = await emailResponse.Content.ReadAsStringAsync(cancellationToken);
                var emails = JsonSerializer.Deserialize<JsonElement[]>(emailContent);
                
                primaryEmail = emails?.FirstOrDefault(e => 
                    e.TryGetProperty("primary", out var isPrimary) && isPrimary.GetBoolean())
                    .GetProperty("email").GetString();
                
                // Fallback to first verified email
                primaryEmail ??= emails?.FirstOrDefault(e => 
                    e.TryGetProperty("verified", out var isVerified) && isVerified.GetBoolean())
                    .GetProperty("email").GetString();
            }

            // Parse name
            var fullName = userData.TryGetProperty("name", out var name) ? name.GetString() : string.Empty;
            var nameParts = fullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            return new OAuthUser
            {
                Id = userData.GetProperty("id").GetInt64().ToString(),
                Email = primaryEmail ?? string.Empty,
                Username = userData.TryGetProperty("login", out var login) ? login.GetString() : null,
                FirstName = nameParts.Length > 0 ? nameParts[0] : null,
                LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : null,
                Avatar = userData.TryGetProperty("avatar_url", out var avatar) ? avatar.GetString() : null,
                RawData = JsonSerializer.Deserialize<Dictionary<string, object>>(userContent) ?? new()
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
        }
    }

    private dynamic? GetProviderSettings(string provider)
    {
        return provider.ToLower() switch
        {
            "google" => _oauthSettings.Google,
            "github" => _oauthSettings.GitHub,
            _ => null
        };
    }

    private List<string> GetProviderScopes(string provider)
    {
        return provider.ToLower() switch
        {
            "google" => _oauthSettings.Google.Scopes,
            "github" => _oauthSettings.GitHub.Scopes,
            _ => new List<string>()
        };
    }
}