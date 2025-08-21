using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DSAGrind.AI.API.Services;

namespace DSAGrind.AI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<AIController> _logger;

    public AIController(IAIService aiService, ILogger<AIController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpPost("hint")]
    public async Task<ActionResult<string>> GenerateHint([FromBody] AIHintRequestDto request)
    {
        try
        {
            var hint = await _aiService.GenerateHintAsync(request.ProblemId, request.UserCode, request.HintLevel);
            return Ok(new { hint });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating hint");
            return StatusCode(500, new { message = "An error occurred while generating hint" });
        }
    }

    [HttpPost("explain")]
    public async Task<ActionResult<string>> ExplainSolution([FromBody] SolutionExplanationRequestDto request)
    {
        try
        {
            var explanation = await _aiService.ExplainSolutionAsync(request.ProblemId, request.Code);
            return Ok(new { explanation });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining solution");
            return StatusCode(500, new { message = "An error occurred while explaining solution" });
        }
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<CodeAnalysisDto>> AnalyzeCode([FromBody] CodeAnalysisRequestDto request)
    {
        try
        {
            var analysis = await _aiService.AnalyzeCodeAsync(request.Code, request.Language, request.ProblemId);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing code");
            return StatusCode(500, new { message = "An error occurred while analyzing code" });
        }
    }

    [HttpPost("optimize")]
    public async Task<ActionResult<string>> OptimizeCode([FromBody] CodeOptimizationRequestDto request)
    {
        try
        {
            var optimizedCode = await _aiService.OptimizeCodeAsync(request.Code, request.Language);
            return Ok(new { optimizedCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing code");
            return StatusCode(500, new { message = "An error occurred while optimizing code" });
        }
    }

    [HttpPost("debug")]
    public async Task<ActionResult<string>> DebugCode([FromBody] CodeDebugRequestDto request)
    {
        try
        {
            var debugSuggestion = await _aiService.DebugCodeAsync(request.Code, request.Language, request.ErrorMessage);
            return Ok(new { debugSuggestion });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error debugging code");
            return StatusCode(500, new { message = "An error occurred while debugging code" });
        }
    }

    [HttpPost("generate-testcases")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<string>> GenerateTestCases([FromBody] TestCaseGenerationRequestDto request)
    {
        try
        {
            var testCases = await _aiService.GenerateTestCasesAsync(request.ProblemDescription, request.Count);
            return Ok(new { testCases });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating test cases");
            return StatusCode(500, new { message = "An error occurred while generating test cases" });
        }
    }

    [HttpPost("estimate-difficulty")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<DifficultyEstimateDto>> EstimateDifficulty([FromBody] string problemDescription)
    {
        try
        {
            var estimate = await _aiService.EstimateDifficultyAsync(problemDescription);
            return Ok(estimate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating difficulty");
            return StatusCode(500, new { message = "An error occurred while estimating difficulty" });
        }
    }
}