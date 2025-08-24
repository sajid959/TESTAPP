using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Unified.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);
        
        await Task.Delay(100); // Simulate processing
        
        return Ok(new
        {
            userId = "demo-user-123",
            username = request.Email?.Split('@')[0] ?? "demouser",
            email = request.Email,
            token = "demo-jwt-token-" + Guid.NewGuid().ToString("N")[..16],
            refreshToken = "demo-refresh-" + Guid.NewGuid().ToString("N")[..16],
            expiresAt = DateTime.UtcNow.AddHours(24),
            message = "Login successful (demo mode)"
        });
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);
        
        await Task.Delay(150); // Simulate processing
        
        return Ok(new
        {
            userId = "demo-new-user-" + Guid.NewGuid().ToString("N")[..8],
            username = request.Username,
            email = request.Email,
            token = "demo-jwt-token-" + Guid.NewGuid().ToString("N")[..16],
            refreshToken = "demo-refresh-" + Guid.NewGuid().ToString("N")[..16],
            expiresAt = DateTime.UtcNow.AddHours(24),
            message = "Registration successful (demo mode)"
        });
    }

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Token refresh attempt");
        
        await Task.Delay(50);
        
        return Ok(new
        {
            token = "demo-new-jwt-token-" + Guid.NewGuid().ToString("N")[..16],
            refreshToken = "demo-new-refresh-" + Guid.NewGuid().ToString("N")[..16],
            expiresAt = DateTime.UtcNow.AddHours(24)
        });
    }

    /// <summary>
    /// Get user profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        _logger.LogInformation("Get profile request");
        
        await Task.Delay(50);
        
        return Ok(new
        {
            userId = "demo-user-123",
            username = "demouser",
            email = "demo@dsagrind.com",
            firstName = "Demo",
            lastName = "User",
            bio = "This is a demo user profile for testing purposes",
            location = "Demo City",
            website = "https://dsagrind.com",
            joinedDate = DateTime.UtcNow.AddDays(-30),
            problemsSolved = 42,
            submissionsCount = 156,
            acceptanceRate = 78.5m
        });
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("Logout request");
        
        await Task.Delay(25);
        
        return Ok(new { message = "Logout successful" });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}