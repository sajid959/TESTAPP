using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DSAGrind.Admin.API.Services;
using DSAGrind.Models.DTOs;

namespace DSAGrind.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
    {
        try
        {
            var dashboard = await _adminService.GetDashboardDataAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin dashboard");
            return StatusCode(500, new { message = "An error occurred while getting dashboard data" });
        }
    }

    [HttpGet("users")]
    public async Task<ActionResult<UserSearchResponseDto>> GetUsers([FromQuery] UserSearchRequestDto request)
    {
        try
        {
            var users = await _adminService.GetUsersAsync(request);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new { message = "An error occurred while getting users" });
        }
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<UserDto>> GetUser(string userId)
    {
        try
        {
            var user = await _adminService.GetUserAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while getting user" });
        }
    }

    [HttpPut("users/{userId}/ban")]
    public async Task<IActionResult> BanUser(string userId, [FromBody] string reason)
    {
        try
        {
            var success = await _adminService.BanUserAsync(userId, reason);
            if (!success)
            {
                return BadRequest(new { message = "Failed to ban user" });
            }
            return Ok(new { message = "User banned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error banning user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while banning user" });
        }
    }

    [HttpPut("users/{userId}/unban")]
    public async Task<IActionResult> UnbanUser(string userId)
    {
        try
        {
            var success = await _adminService.UnbanUserAsync(userId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to unban user" });
            }
            return Ok(new { message = "User unbanned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unbanning user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while unbanning user" });
        }
    }

    [HttpGet("problems/pending")]
    public async Task<ActionResult<List<ProblemDto>>> GetPendingProblems()
    {
        try
        {
            var problems = await _adminService.GetPendingProblemsAsync();
            return Ok(problems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending problems");
            return StatusCode(500, new { message = "An error occurred while getting pending problems" });
        }
    }

    [HttpPut("problems/{problemId}/approve")]
    public async Task<IActionResult> ApproveProblem(string problemId)
    {
        try
        {
            var success = await _adminService.ApproveProblemAsync(problemId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to approve problem" });
            }
            return Ok(new { message = "Problem approved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving problem {ProblemId}", problemId);
            return StatusCode(500, new { message = "An error occurred while approving problem" });
        }
    }

    [HttpPut("problems/{problemId}/reject")]
    public async Task<IActionResult> RejectProblem(string problemId, [FromBody] string reason)
    {
        try
        {
            var success = await _adminService.RejectProblemAsync(problemId, reason);
            if (!success)
            {
                return BadRequest(new { message = "Failed to reject problem" });
            }
            return Ok(new { message = "Problem rejected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting problem {ProblemId}", problemId);
            return StatusCode(500, new { message = "An error occurred while rejecting problem" });
        }
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<AdminAnalyticsDto>> GetAnalytics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var analytics = await _adminService.GetAnalyticsAsync(from, to);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics");
            return StatusCode(500, new { message = "An error occurred while getting analytics" });
        }
    }

    [HttpPost("system/backup")]
    public async Task<IActionResult> CreateBackup()
    {
        try
        {
            var backupId = await _adminService.CreateBackupAsync();
            return Ok(new { backupId, message = "Backup created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backup");
            return StatusCode(500, new { message = "An error occurred while creating backup" });
        }
    }

    [HttpGet("system/health")]
    public async Task<ActionResult<SystemHealthDto>> GetSystemHealth()
    {
        try
        {
            var health = await _adminService.GetSystemHealthAsync();
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system health");
            return StatusCode(500, new { message = "An error occurred while getting system health" });
        }
    }
}