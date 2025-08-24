using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    private readonly ILogger<SubmissionsController> _logger;

    public SubmissionsController(ILogger<SubmissionsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Submit code for execution and testing
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitCode([FromBody] SubmissionRequest request)
    {
        _logger.LogInformation("Code submission for problem: {ProblemId}, language: {Language}", request.ProblemId, request.Language);
        
        // Simulate code execution time
        await Task.Delay(Random.Shared.Next(800, 2000));
        
        var isAccepted = Random.Shared.Next(1, 101) <= 78; // 78% pass rate
        
        var result = new
        {
            submissionId = Guid.NewGuid().ToString(),
            status = isAccepted ? "Accepted" : Random.Shared.Next(1, 5) switch
            {
                1 => "Wrong Answer",
                2 => "Time Limit Exceeded", 
                3 => "Runtime Error",
                4 => "Compilation Error",
                _ => "Memory Limit Exceeded"
            },
            runtime = Random.Shared.Next(8, 150) + "ms",
            memory = Random.Shared.Next(10, 45) + ".2MB",
            passedTestCases = isAccepted ? "15/15" : $"{Random.Shared.Next(5, 14)}/15",
            submittedAt = DateTime.UtcNow,
            code = request.Code,
            language = request.Language,
            problemId = request.ProblemId
        };
        
        if (isAccepted)
        {
            return Ok(new
            {
                result,
                message = "Congratulations! Your solution has been accepted.",
                performance = new
                {
                    runtimePercentile = Random.Shared.Next(45, 98) + Random.Shared.NextSingle(),
                    memoryPercentile = Random.Shared.Next(35, 95) + Random.Shared.NextSingle(),
                    timeComplexity = "O(n)",
                    spaceComplexity = "O(n)"
                }
            });
        }
        else
        {
            return Ok(new
            {
                result,
                error = new
                {
                    testCase = "Test Case " + Random.Shared.Next(3, 12),
                    input = "[3,2,4], target = 6",
                    expected = "[1,2]", 
                    actual = result.status == "Wrong Answer" ? "[0,2]" : null,
                    errorMessage = result.status switch
                    {
                        "Time Limit Exceeded" => "Your solution exceeded the time limit of 1000ms",
                        "Runtime Error" => "IndexOutOfRangeException: Index was outside the bounds of the array",
                        "Compilation Error" => "CS1002: Syntax error, ';' expected",
                        "Memory Limit Exceeded" => "Your solution used more than 256MB of memory",
                        _ => "Your output doesn't match the expected result for the given input"
                    }
                }
            });
        }
    }

    /// <summary>
    /// Run code against sample test cases
    /// </summary>
    [HttpPost("run")]
    public async Task<IActionResult> RunCode([FromBody] RunCodeRequest request)
    {
        _logger.LogInformation("Running code for problem: {ProblemId}, language: {Language}", request.ProblemId, request.Language);
        
        await Task.Delay(Random.Shared.Next(300, 800));
        
        var testResults = new[]
        {
            new {
                testCase = 1,
                input = "[2,7,11,15], target = 9",
                expected = "[0,1]",
                actual = "[0,1]",
                passed = true,
                runtime = "12ms"
            },
            new {
                testCase = 2, 
                input = "[3,2,4], target = 6",
                expected = "[1,2]",
                actual = "[1,2]",
                passed = true,
                runtime = "8ms"
            },
            new {
                testCase = 3,
                input = "[3,3], target = 6", 
                expected = "[0,1]",
                actual = "[0,1]",
                passed = true,
                runtime = "6ms"
            }
        };
        
        var allPassed = testResults.All(t => t.passed);
        
        return Ok(new
        {
            success = allPassed,
            testResults,
            summary = new
            {
                totalTests = testResults.Length,
                passedTests = testResults.Count(t => t.passed),
                avgRuntime = testResults.Average(t => int.Parse(t.runtime.Replace("ms", ""))) + "ms"
            },
            message = allPassed ? "All test cases passed! Ready to submit." : "Some test cases failed. Please review your code."
        });
    }

    /// <summary>
    /// Get user's submission history
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetSubmissionHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Getting submission history: page={Page}", page);
        
        await Task.Delay(150);
        
        var submissions = Enumerable.Range(1, 15).Select(i => new
        {
            id = $"sub-{i}",
            problemTitle = $"Problem {Random.Shared.Next(1, 100)}",
            status = Random.Shared.Next(1, 4) switch
            {
                1 => "Accepted",
                2 => "Wrong Answer", 
                3 => "Time Limit Exceeded",
                _ => "Runtime Error"
            },
            language = Random.Shared.Next(1, 4) switch
            {
                1 => "C#",
                2 => "Python",
                3 => "JavaScript", 
                _ => "Java"
            },
            runtime = Random.Shared.Next(8, 200) + "ms",
            submittedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 30))
        }).ToArray();
        
        return Ok(new
        {
            submissions,
            totalCount = submissions.Length,
            pageNumber = page,
            pageSize = pageSize,
            acceptanceRate = Math.Round((double)submissions.Count(s => s.status == "Accepted") / submissions.Length * 100, 1)
        });
    }
}

public class SubmissionRequest
{
    public string ProblemId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}

public class RunCodeRequest
{
    public string ProblemId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}