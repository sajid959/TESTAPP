using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly ILogger<AIController> _logger;

    public AIController(ILogger<AIController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate hints for a coding problem
    /// </summary>
    [HttpPost("hint")]
    public async Task<IActionResult> GenerateHint([FromBody] HintRequest request)
    {
        _logger.LogInformation("Generating hint for problem: {ProblemId}, level: {Level}", request.ProblemId, request.HintLevel);
        
        await Task.Delay(500); // Simulate AI processing time
        
        var hints = new Dictionary<int, string>
        {
            [1] = "Think about what data structure would help you look up values efficiently. Consider the relationship between the target and each number in the array.",
            [2] = "A hash table (dictionary) can store numbers you've seen before. As you iterate through the array, check if the complement (target - current number) exists in your hash table.",
            [3] = "Here's the approach: Create a hash map, iterate through the array once, and for each number, check if (target - number) exists in the map. If yes, return the indices. If no, add the current number and its index to the map."
        };
        
        var hint = hints.GetValueOrDefault(request.HintLevel, "Try breaking down the problem into smaller steps and think about the most efficient approach.");
        
        return Ok(new { hint, hintLevel = request.HintLevel, nextHintAvailable = request.HintLevel < 3 });
    }

    /// <summary>
    /// Explain a solution with detailed analysis
    /// </summary>
    [HttpPost("explain")]
    public async Task<IActionResult> ExplainSolution([FromBody] ExplainRequest request)
    {
        _logger.LogInformation("Explaining solution for problem: {ProblemId}", request.ProblemId);
        
        await Task.Delay(800); // Simulate AI processing time
        
        return Ok(new
        {
            explanation = "This solution uses a Hash Map approach:\n\n" +
                         "1. **Algorithm**: We iterate through the array once, and for each element, we check if its complement (target - current element) exists in our hash map.\n\n" +
                         "2. **Time Complexity**: O(n) - We traverse the array exactly once\n\n" +
                         "3. **Space Complexity**: O(n) - In the worst case, we store all n elements in the hash map\n\n" +
                         "4. **Key Insight**: Instead of checking all pairs (which would be O(n²)), we use the hash map to achieve O(1) lookups",
            timeComplexity = "O(n)",
            spaceComplexity = "O(n)",
            approach = "Hash Map"
        });
    }

    /// <summary>
    /// Analyze code for complexity and potential improvements
    /// </summary>
    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeCode([FromBody] AnalyzeRequest request)
    {
        _logger.LogInformation("Analyzing code for problem: {ProblemId}, language: {Language}", request.ProblemId, request.Language);
        
        await Task.Delay(600);
        
        return Ok(new
        {
            analysis = new
            {
                timeComplexity = "O(n)",
                spaceComplexity = "O(n)",
                codeQuality = "Good",
                readability = 8.5m,
                efficiency = 9.2m
            },
            suggestions = new[]
            {
                "Consider edge cases: empty array, single element, no solution",
                "Add input validation for null arrays",
                "Variable names are clear and descriptive - good practice"
            }
        });
    }
}

public class HintRequest
{
    public string ProblemId { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public int HintLevel { get; set; } = 1;
}

public class ExplainRequest
{
    public string ProblemId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class AnalyzeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string ProblemId { get; set; } = string.Empty;
}