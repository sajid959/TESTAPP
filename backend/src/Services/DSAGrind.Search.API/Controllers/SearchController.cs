using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DSAGrind.Search.API.Services;

namespace DSAGrind.Search.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<SearchResultDto>> Search([FromBody] SearchRequestDto request)
    {
        try
        {
            var result = await _searchService.SearchAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search");
            return StatusCode(500, new { message = "An error occurred while searching" });
        }
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<List<string>>> GetSuggestions([FromQuery] string query, [FromQuery] int count = 5)
    {
        try
        {
            var suggestions = await _searchService.GetSearchSuggestionsAsync(query, count);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search suggestions");
            return StatusCode(500, new { message = "An error occurred while getting suggestions" });
        }
    }

    [HttpGet("similar/{problemId}")]
    public async Task<ActionResult<List<ProblemDto>>> GetSimilarProblems(string problemId, [FromQuery] int count = 5)
    {
        try
        {
            var problems = await _searchService.GetSimilarProblemsAsync(problemId, count);
            return Ok(problems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar problems");
            return StatusCode(500, new { message = "An error occurred while getting similar problems" });
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

            var recommendations = await _searchService.GetRecommendationsAsync(userId, count);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations");
            return StatusCode(500, new { message = "An error occurred while getting recommendations" });
        }
    }

    [HttpPost("by-tags")]
    public async Task<ActionResult<List<ProblemDto>>> SearchByTags([FromBody] List<string> tags, [FromQuery] int count = 20)
    {
        try
        {
            var problems = await _searchService.SearchByTagsAsync(tags, count);
            return Ok(problems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching by tags");
            return StatusCode(500, new { message = "An error occurred while searching by tags" });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPost("index/{problemId}")]
    public async Task<IActionResult> IndexProblem(string problemId)
    {
        try
        {
            var success = await _searchService.IndexProblemAsync(problemId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to index problem" });
            }
            
            return Ok(new { message = "Problem indexed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing problem {ProblemId}", problemId);
            return StatusCode(500, new { message = "An error occurred while indexing problem" });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("index/{problemId}")]
    public async Task<IActionResult> UpdateProblemIndex(string problemId)
    {
        try
        {
            var success = await _searchService.UpdateProblemIndexAsync(problemId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to update problem index" });
            }
            
            return Ok(new { message = "Problem index updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating problem index {ProblemId}", problemId);
            return StatusCode(500, new { message = "An error occurred while updating problem index" });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("index/{problemId}")]
    public async Task<IActionResult> DeleteProblemIndex(string problemId)
    {
        try
        {
            var success = await _searchService.DeleteProblemIndexAsync(problemId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to delete problem index" });
            }
            
            return Ok(new { message = "Problem index deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting problem index {ProblemId}", problemId);
            return StatusCode(500, new { message = "An error occurred while deleting problem index" });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpGet("analytics")]
    public async Task<ActionResult<SearchAnalyticsDto>> GetAnalytics()
    {
        try
        {
            var analytics = await _searchService.GetSearchAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search analytics");
            return StatusCode(500, new { message = "An error occurred while getting analytics" });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}