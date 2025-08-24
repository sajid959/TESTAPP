using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Unified.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProblemsController : ControllerBase
{
    private readonly ILogger<ProblemsController> _logger;

    public ProblemsController(ILogger<ProblemsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all problems with optional filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProblems(
        [FromQuery] string? search = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] string? tag = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Get problems: search={Search}, difficulty={Difficulty}, page={Page}", search, difficulty, page);
        
        await Task.Delay(100);

        var problems = new[]
        {
            new {
                id = "prob-1",
                title = "Two Sum",
                slug = "two-sum",
                difficulty = "Easy",
                tags = new[] { "Array", "Hash Table" },
                acceptanceRate = 85.2m,
                totalSubmissions = 1250000,
                acceptedSubmissions = 1065000,
                description = "Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.",
                sampleInput = "[2,7,11,15], target = 9",
                sampleOutput = "[0,1]"
            },
            new {
                id = "prob-2", 
                title = "Reverse Linked List",
                slug = "reverse-linked-list",
                difficulty = "Easy",
                tags = new[] { "Linked List", "Recursion" },
                acceptanceRate = 78.5m,
                totalSubmissions = 890000,
                acceptedSubmissions = 698650,
                description = "Given the head of a singly linked list, reverse the list, and return the reversed list.",
                sampleInput = "head = [1,2,3,4,5]",
                sampleOutput = "[5,4,3,2,1]"
            },
            new {
                id = "prob-3",
                title = "Valid Parentheses",
                slug = "valid-parentheses", 
                difficulty = "Easy",
                tags = new[] { "String", "Stack" },
                acceptanceRate = 92.1m,
                totalSubmissions = 756000,
                acceptedSubmissions = 696216,
                description = "Given a string s containing just the characters '(', ')', '{', '}', '[' and ']', determine if the input string is valid.",
                sampleInput = "s = \"()\"",
                sampleOutput = "true"
            },
            new {
                id = "prob-4",
                title = "Binary Search",
                slug = "binary-search",
                difficulty = "Easy", 
                tags = new[] { "Array", "Binary Search" },
                acceptanceRate = 89.7m,
                totalSubmissions = 532000,
                acceptedSubmissions = 477204,
                description = "Given an array of integers nums which is sorted in ascending order, and an integer target, write a function to search target in nums.",
                sampleInput = "nums = [-1,0,3,5,9,12], target = 9",
                sampleOutput = "4"
            },
            new {
                id = "prob-5",
                title = "Merge Two Sorted Lists", 
                slug = "merge-two-sorted-lists",
                difficulty = "Easy",
                tags = new[] { "Linked List", "Recursion" },
                acceptanceRate = 64.3m,
                totalSubmissions = 1180000,
                acceptedSubmissions = 758740,
                description = "You are given the heads of two sorted linked lists list1 and list2. Merge the two lists into one sorted list.",
                sampleInput = "list1 = [1,2,4], list2 = [1,3,4]",
                sampleOutput = "[1,1,2,3,4,4]"
            }
        };

        var filteredProblems = problems.AsEnumerable();
        
        if (!string.IsNullOrEmpty(difficulty))
            filteredProblems = filteredProblems.Where(p => p.difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase));
            
        if (!string.IsNullOrEmpty(search))
            filteredProblems = filteredProblems.Where(p => p.title.Contains(search, StringComparison.OrdinalIgnoreCase));
            
        if (!string.IsNullOrEmpty(tag))
            filteredProblems = filteredProblems.Where(p => p.tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)));

        var result = filteredProblems.ToList();
        
        return Ok(new
        {
            problems = result,
            totalCount = result.Count,
            pageNumber = page,
            pageSize = pageSize,
            totalPages = (int)Math.Ceiling((double)result.Count / pageSize)
        });
    }

    /// <summary>
    /// Get a specific problem by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProblem(string id)
    {
        _logger.LogInformation("Get problem: {ProblemId}", id);
        
        await Task.Delay(50);
        
        return Ok(new
        {
            id = id,
            title = "Two Sum",
            slug = "two-sum",
            difficulty = "Easy",
            tags = new[] { "Array", "Hash Table" },
            description = "Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.\n\nYou can return the answer in any order.\n\nYou may assume that each input would have exactly one solution, and you may not use the same element twice.",
            sampleInput = "[2,7,11,15], target = 9",
            sampleOutput = "[0,1]",
            explanation = "Because nums[0] + nums[1] == 9, we return [0, 1].",
            constraints = new[]
            {
                "2 <= nums.length <= 10^4",
                "-10^9 <= nums[i] <= 10^9",
                "-10^9 <= target <= 10^9",
                "Only one valid answer exists."
            },
            codeTemplate = "class Solution {\n    public int[] TwoSum(int[] nums, int target) {\n        // Your code here\n    }\n}",
            testCases = new[]
            {
                new { input = "[2,7,11,15], target = 9", output = "[0,1]" },
                new { input = "[3,2,4], target = 6", output = "[1,2]" },
                new { input = "[3,3], target = 6", output = "[0,1]" }
            },
            acceptanceRate = 85.2m,
            totalSubmissions = 1250000,
            acceptedSubmissions = 1065000
        });
    }

    /// <summary>
    /// Get problem by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetProblemBySlug(string slug)
    {
        _logger.LogInformation("Get problem by slug: {Slug}", slug);
        
        await Task.Delay(50);
        
        return Ok(new
        {
            id = "prob-1",
            title = slug.Replace("-", " ").ToTitleCase(),
            slug = slug,
            difficulty = "Easy",
            tags = new[] { "Array", "Hash Table" },
            description = $"This is the problem description for {slug.Replace("-", " ")}.",
            sampleInput = "Example input",
            sampleOutput = "Example output"
        });
    }

    /// <summary>
    /// Get popular tags
    /// </summary>
    [HttpGet("tags")]
    public async Task<IActionResult> GetTags([FromQuery] int limit = 20)
    {
        _logger.LogInformation("Get popular tags with limit: {Limit}", limit);
        
        await Task.Delay(25);
        
        var tags = new[]
        {
            "Array", "String", "Hash Table", "Dynamic Programming", "Math",
            "Tree", "Depth-First Search", "Binary Search", "Two Pointers", "Greedy",
            "Breadth-First Search", "Stack", "Design", "Heap (Priority Queue)", "Graph",
            "Simulation", "Backtracking", "Counting", "Sliding Window", "Union Find"
        };
        
        return Ok(tags.Take(limit).ToArray());
    }

    /// <summary>
    /// Get random problems by difficulty
    /// </summary>
    [HttpGet("random")]
    public async Task<IActionResult> GetRandomProblems([FromQuery] string difficulty = "Easy", [FromQuery] int count = 1)
    {
        _logger.LogInformation("Get random problems: difficulty={Difficulty}, count={Count}", difficulty, count);
        
        await Task.Delay(75);
        
        var randomProblems = Enumerable.Range(1, count).Select(i => new
        {
            id = $"random-{i}",
            title = $"Random {difficulty} Problem {i}",
            slug = $"random-{difficulty.ToLower()}-problem-{i}",
            difficulty = difficulty,
            tags = new[] { "Array", "Algorithm" },
            acceptanceRate = Random.Shared.Next(60, 95) + Random.Shared.NextSingle()
        }).ToArray();
        
        return Ok(randomProblems);
    }
}

public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        return string.Join(" ", input.Split(' ').Select(word => 
            char.ToUpper(word[0]) + word[1..].ToLower()));
    }
}