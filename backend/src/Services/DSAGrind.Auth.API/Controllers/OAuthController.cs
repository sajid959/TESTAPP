using Microsoft.AspNetCore.Mvc;
using DSAGrind.Auth.API.Services;
using DSAGrind.Models.DTOs;

namespace DSAGrind.Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<OAuthController> _logger;

    public OAuthController(IAuthService authService, ILogger<OAuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Get OAuth authorization URL for the specified provider
    /// </summary>
    [HttpGet("{provider}/url")]
    public async Task<ActionResult<string>> GetAuthorizationUrl(string provider, [FromQuery] string? state = null)
    {
        try
        {
            if (!IsValidProvider(provider))
            {
                return BadRequest(new { message = "Unsupported OAuth provider" });
            }

            var stateValue = state ?? Guid.NewGuid().ToString();
            var authUrl = await _authService.GenerateOAuthUrlAsync(provider, stateValue);
            
            return Ok(new { url = authUrl, state = stateValue });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OAuth URL for provider {Provider}", provider);
            return StatusCode(500, new { message = "An error occurred while generating authorization URL" });
        }
    }

    /// <summary>
    /// Handle OAuth callback and authenticate user
    /// </summary>
    [HttpPost("{provider}/callback")]
    public async Task<ActionResult<AuthResponseDto>> HandleCallback(string provider, [FromBody] OAuthCallbackDto callback)
    {
        try
        {
            if (!IsValidProvider(provider))
            {
                return BadRequest(new { message = "Unsupported OAuth provider" });
            }

            if (!string.IsNullOrEmpty(callback.Error))
            {
                _logger.LogWarning("OAuth error for provider {Provider}: {Error} - {ErrorDescription}", 
                    provider, callback.Error, callback.ErrorDescription);
                return BadRequest(new { message = $"OAuth error: {callback.ErrorDescription ?? callback.Error}" });
            }

            if (string.IsNullOrEmpty(callback.Code))
            {
                return BadRequest(new { message = "Authorization code is required" });
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.OAuthLoginAsync(provider, callback.Code, callback.State, ipAddress);
            
            SetRefreshTokenCookie(result.RefreshToken);
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth callback for provider {Provider}", provider);
            return StatusCode(500, new { message = "An error occurred during OAuth authentication" });
        }
    }

    /// <summary>
    /// Get supported OAuth providers
    /// </summary>
    [HttpGet("providers")]
    public IActionResult GetProviders()
    {
        var providers = new[]
        {
            new { name = "google", displayName = "Google", icon = "google" },
            new { name = "github", displayName = "GitHub", icon = "github" }
        };
        
        return Ok(providers);
    }

    private static bool IsValidProvider(string provider)
    {
        var validProviders = new[] { "google", "github" };
        return validProviders.Contains(provider.ToLower());
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth"
        };
        
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private string GetIpAddress()
    {
        return Request.Headers.ContainsKey("X-Forwarded-For") 
            ? Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}