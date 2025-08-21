using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DSAGrind.Submissions.API.Services;

namespace DSAGrind.Submissions.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;
    private readonly ILogger<SubmissionsController> _logger;

    public SubmissionsController(ISubmissionService submissionService, ILogger<SubmissionsController> logger)
    {
        _submissionService = submissionService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubmissionDto>> GetSubmission(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var submission = await _submissionService.GetSubmissionAsync(id, userId);
            if (submission == null)
            {
                return NotFound();
            }
            
            return Ok(submission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submission {SubmissionId}", id);
            return StatusCode(500, new { message = "An error occurred while getting submission" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<SubmissionDto>>> GetSubmissions([FromQuery] string? problemId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var submissions = await _submissionService.GetUserSubmissionsAsync(userId, problemId, page, pageSize);
            return Ok(submissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user submissions");
            return StatusCode(500, new { message = "An error occurred while getting submissions" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<SubmissionDto>> CreateSubmission([FromBody] CreateSubmissionRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var submission = await _submissionService.CreateSubmissionAsync(request, userId);
            return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, submission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating submission");
            return StatusCode(500, new { message = "An error occurred while creating submission" });
        }
    }

    [HttpPost("execute")]
    public async Task<ActionResult<CodeExecutionResultDto>> ExecuteCode([FromBody] CodeExecutionRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _submissionService.ExecuteCodeAsync(request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing code");
            return StatusCode(500, new { message = "An error occurred while executing code" });
        }
    }

    [HttpPost("test")]
    public async Task<ActionResult<CodeExecutionResultDto>> TestCode([FromBody] CodeTestRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _submissionService.TestCodeAsync(request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing code");
            return StatusCode(500, new { message = "An error occurred while testing code" });
        }
    }

    [HttpGet("languages")]
    [AllowAnonymous]
    public async Task<ActionResult<List<string>>> GetSupportedLanguages()
    {
        try
        {
            var languages = await _submissionService.GetSupportedLanguagesAsync();
            return Ok(languages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported languages");
            return StatusCode(500, new { message = "An error occurred while getting supported languages" });
        }
    }

    [HttpGet("leaderboard/{problemId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<SubmissionDto>>> GetLeaderboard(string problemId, [FromQuery] int limit = 10)
    {
        try
        {
            var leaderboard = await _submissionService.GetLeaderboardAsync(problemId, limit);
            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard for problem {ProblemId}", problemId);
            return StatusCode(500, new { message = "An error occurred while getting leaderboard" });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}