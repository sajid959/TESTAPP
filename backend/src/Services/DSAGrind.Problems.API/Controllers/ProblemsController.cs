using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DSAGrind.Problems.API.Services;
using DSAGrind.Models.DTOs;

namespace DSAGrind.Problems.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProblemsController : ControllerBase
{
    private readonly IProblemService _problemService;
    private readonly ILogger<ProblemsController> _logger;

    public ProblemsController(IProblemService problemService, ILogger<ProblemsController> logger)
    {
        _problemService = problemService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ProblemSearchResponseDto>> GetProblems([FromQuery] ProblemSearchRequestDto request)
    {
        try
        {
            var result = await _problemService.SearchProblemsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching problems");
            return StatusCode(500, new { message = "An error occurred while searching problems" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProblemDto>> GetProblem(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var problem = await _problemService.GetProblemAsync(id, userId);
            
            if (problem == null)
            {
                return NotFound();
            }
            
            return Ok(problem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting problem {ProblemId}", id);
            return StatusCode(500, new { message = "An error occurred while getting problem" });
        }
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProblemDto>> GetProblemBySlug(string slug)
    {
        try
        {
            var userId = GetCurrentUserId();
            var problem = await _problemService.GetProblemBySlugAsync(slug, userId);
            
            if (problem == null)
            {
                return NotFound();
            }
            
            return Ok(problem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting problem by slug {Slug}", slug);
            return StatusCode(500, new { message = "An error occurred while getting problem" });
        }
    }

    [HttpGet("random")]
    public async Task<ActionResult<List<ProblemDto>>> GetRandomProblems([FromQuery] int count = 5, [FromQuery] string? difficulty = null)
    {
        try
        {
            var problems = await _problemService.GetRandomProblemsAsync(count, difficulty);
            return Ok(problems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting random problems");
            return StatusCode(500, new { message = "An error occurred while getting random problems" });
        }
    }

    [Authorize]
    [HttpGet("recommendations")]
    public async Task<ActionResult<List<ProblemDto>>> GetRecommendations([FromQuery] int count = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var problems = await _problemService.GetRecommendedProblemsAsync(userId, count);
            return Ok(problems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended problems");
            return StatusCode(500, new { message = "An error occurred while getting recommendations" });
        }
    }

    [HttpGet("{id}/similar")]
    public async Task<ActionResult<List<ProblemDto>>> GetSimilarProblems(string id, [FromQuery] int count = 5)
    {
        try
        {
            var problems = await _problemService.GetSimilarProblemsAsync(id, count);
            return Ok(problems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar problems for {ProblemId}", id);
            return StatusCode(500, new { message = "An error occurred while getting similar problems" });
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ProblemDto>> CreateProblem([FromBody] CreateProblemRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var problem = await _problemService.CreateProblemAsync(request, userId);
            return CreatedAtAction(nameof(GetProblem), new { id = problem.Id }, problem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating problem");
            return StatusCode(500, new { message = "An error occurred while creating problem" });
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<ProblemDto>> UpdateProblem(string id, [FromBody] UpdateProblemRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var problem = await _problemService.UpdateProblemAsync(id, request, userId);
            if (problem == null)
            {
                return NotFound();
            }
            
            return Ok(problem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating problem {ProblemId}", id);
            return StatusCode(500, new { message = "An error occurred while updating problem" });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveProblem(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _problemService.ApproveProblemAsync(id, userId);
            if (!success)
            {
                return NotFound();
            }
            
            return Ok(new { message = "Problem approved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving problem {ProblemId}", id);
            return StatusCode(500, new { message = "An error occurred while approving problem" });
        }
    }

    [Authorize]
    [HttpPost("{id}/like")]
    public async Task<IActionResult> LikeProblem(string id, [FromBody] bool isLike = true)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _problemService.LikeProblemAsync(id, userId, isLike);
            if (!success)
            {
                return NotFound();
            }
            
            return Ok(new { message = isLike ? "Problem liked" : "Problem disliked" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking problem {ProblemId}", id);
            return StatusCode(500, new { message = "An error occurred while updating problem rating" });
        }
    }

    [Authorize]
    [HttpPost("bulk-import")]
    public async Task<ActionResult<List<ProblemDto>>> BulkImportProblems([FromBody] BulkImportRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var problems = await _problemService.BulkImportProblemsAsync(request, userId);
            return Ok(problems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk importing problems");
            return StatusCode(500, new { message = "An error occurred while importing problems" });
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProblem(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _problemService.DeleteProblemAsync(id, userId);
            if (!success)
            {
                return NotFound();
            }
            
            return Ok(new { message = "Problem deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting problem {ProblemId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting problem" });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}