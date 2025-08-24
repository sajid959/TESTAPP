using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Unified.API.Controllers;

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
            explanation = "This solution uses the Two Pointer technique combined with a Hash Map approach:\n\n" +
                         "1. **Algorithm**: We iterate through the array once, and for each element, we check if its complement (target - current element) exists in our hash map.\n\n" +
                         "2. **Time Complexity**: O(n) - We traverse the array exactly once\n\n" +
                         "3. **Space Complexity**: O(n) - In the worst case, we store all n elements in the hash map\n\n" +
                         "4. **Key Insight**: Instead of checking all pairs (which would be O(n²)), we use the hash map to achieve O(1) lookups\n\n" +
                         "5. **Implementation Details**: \n" +
                         "   - For each number at index i, we calculate complement = target - nums[i]\n" +
                         "   - If complement exists in our map, we found our pair\n" +
                         "   - Otherwise, we add nums[i] and its index to the map",
            timeComplexity = "O(n)",
            spaceComplexity = "O(n)",
            approach = "Hash Map / Two Pass",
            keyInsights = new[]
            {
                "Use complement lookup instead of nested loops",
                "Hash map provides O(1) lookup time", 
                "Store indices as values for easy retrieval"
            }
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
                efficiency = 9.2m,
                correctness = "Likely Correct"
            },
            suggestions = new[]
            {
                "Consider edge cases: empty array, single element, no solution",
                "Add input validation for null arrays",
                "Variable names are clear and descriptive - good practice",
                "Consider using more descriptive variable names than 'i' and 'j'"
            },
            strengths = new[]
            {
                "Efficient hash map approach",
                "Clean and readable code structure", 
                "Good use of early return pattern"
            },
            potentialIssues = new[]
            {
                "No null checking for input array",
                "Assumes exactly one solution exists (as per problem constraint)"
            }
        });
    }

    /// <summary>
    /// Get code optimization suggestions
    /// </summary>
    [HttpPost("optimize")]
    public async Task<IActionResult> OptimizeCode([FromBody] OptimizeRequest request)
    {
        _logger.LogInformation("Optimizing code for language: {Language}", request.Language);
        
        await Task.Delay(700);
        
        var optimizedCode = request.Language.ToLower() switch
        {
            "csharp" => @"public int[] TwoSum(int[] nums, int target) {
    if (nums == null || nums.Length < 2) 
        return new int[0]; // Handle edge cases
        
    var numToIndex = new Dictionary<int, int>(nums.Length);
    
    for (int i = 0; i < nums.Length; i++) {
        int complement = target - nums[i];
        
        if (numToIndex.TryGetValue(complement, out int complementIndex)) {
            return new int[] { complementIndex, i };
        }
        
        numToIndex[nums[i]] = i; // Only add if not found
    }
    
    return new int[0]; // No solution found
}",
            "python" => @"def twoSum(self, nums: List[int], target: int) -> List[int]:
    if not nums or len(nums) < 2:
        return []
    
    num_to_index = {}
    
    for i, num in enumerate(nums):
        complement = target - num
        
        if complement in num_to_index:
            return [num_to_index[complement], i]
        
        num_to_index[num] = i
    
    return []  # No solution found",
            _ => "// Optimized code would be provided based on the specific language"
        };
        
        return Ok(new
        {
            optimizedCode,
            improvements = new[]
            {
                "Added null/empty array validation",
                "Pre-sized dictionary for better performance",
                "Used TryGetValue for safer dictionary access",
                "Added explicit return for no solution case",
                "More descriptive variable names"
            },
            performanceGains = new
            {
                timeComplexity = "No change - still O(n)",
                spaceComplexity = "Slightly better due to pre-sizing",
                memoryAllocations = "Reduced by pre-sizing dictionary",
                safetyImprovements = "Added null checks and error handling"
            }
        });
    }

    /// <summary>
    /// Generate test cases for a problem
    /// </summary>
    [HttpPost("test-cases")]
    public async Task<IActionResult> GenerateTestCases([FromBody] TestCaseRequest request)
    {
        _logger.LogInformation("Generating test cases for problem: {ProblemId}", request.ProblemId);
        
        await Task.Delay(400);
        
        return Ok(new
        {
            testCases = new[]
            {
                new { 
                    name = "Basic Case", 
                    input = new { nums = new[] { 2, 7, 11, 15 }, target = 9 }, 
                    expectedOutput = new[] { 0, 1 },
                    explanation = "nums[0] + nums[1] = 2 + 7 = 9"
                },
                new { 
                    name = "Multiple Valid Pairs", 
                    input = new { nums = new[] { 3, 2, 4 }, target = 6 }, 
                    expectedOutput = new[] { 1, 2 },
                    explanation = "nums[1] + nums[2] = 2 + 4 = 6"
                },
                new { 
                    name = "Same Element Twice", 
                    input = new { nums = new[] { 3, 3 }, target = 6 }, 
                    expectedOutput = new[] { 0, 1 },
                    explanation = "nums[0] + nums[1] = 3 + 3 = 6"
                },
                new { 
                    name = "Negative Numbers", 
                    input = new { nums = new[] { -1, -2, -3, -4, -5 }, target = -8 }, 
                    expectedOutput = new[] { 2, 4 },
                    explanation = "nums[2] + nums[4] = -3 + (-5) = -8"
                },
                new { 
                    name = "Large Numbers", 
                    input = new { nums = new[] { 1000000, 2000000, 3000000 }, target = 5000000 }, 
                    expectedOutput = new[] { 1, 2 },
                    explanation = "nums[1] + nums[2] = 2000000 + 3000000 = 5000000"
                }
            },
            edgeCases = new[]
            {
                "Minimum array size (2 elements)",
                "All negative numbers", 
                "Mix of positive and negative numbers",
                "Large integer values near int.MaxValue",
                "Zero as target or array element"
            },
            coverage = new
            {
                basicFunctionality = true,
                edgeCases = true,
                boundaryConditions = true,
                errorConditions = false
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

public class OptimizeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}

public class TestCaseRequest
{
    public string ProblemId { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string Constraints { get; set; } = string.Empty;
}