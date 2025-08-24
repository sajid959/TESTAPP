using DSAGrind.Models.DTOs;

namespace DSAGrind.Problems.API.Services;

public class MockProblemService : IProblemService
{
    private readonly ILogger<MockProblemService> _logger;

    public MockProblemService(ILogger<MockProblemService> logger)
    {
        _logger = logger;
    }

    public Task<ProblemSearchResponseDto> SearchProblemsAsync(ProblemSearchRequestDto request)
    {
        _logger.LogInformation("Mock search problems with query: {Query}", request.SearchTerm);
        
        var mockProblems = new List<ProblemDto>
        {
            new ProblemDto
            {
                Id = "problem-1",
                Title = "Two Sum",
                Slug = "two-sum",
                Description = "Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.",
                Difficulty = "Easy",
                Tags = new List<string> { "Array", "Hash Table" },
                SampleInput = "[2,7,11,15], target = 9",
                SampleOutput = "[0,1]",
                AcceptanceRate = 85.2m,
                TotalSubmissions = 1250000,
                AcceptedSubmissions = 1065000
            },
            new ProblemDto
            {
                Id = "problem-2", 
                Title = "Reverse Linked List",
                Slug = "reverse-linked-list",
                Description = "Given the head of a singly linked list, reverse the list, and return the reversed list.",
                Difficulty = "Easy",
                Tags = new List<string> { "Linked List", "Recursion" },
                SampleInput = "head = [1,2,3,4,5]",
                SampleOutput = "[5,4,3,2,1]",
                AcceptanceRate = 78.5m,
                TotalSubmissions = 890000,
                AcceptedSubmissions = 698650
            },
            new ProblemDto
            {
                Id = "problem-3",
                Title = "Binary Tree Inorder Traversal", 
                Slug = "binary-tree-inorder-traversal",
                Description = "Given the root of a binary tree, return the inorder traversal of its nodes' values.",
                Difficulty = "Easy",
                Tags = new List<string> { "Stack", "Tree", "Depth-First Search" },
                SampleInput = "root = [1,null,2,3]",
                SampleOutput = "[1,3,2]",
                AcceptanceRate = 73.8m,
                TotalSubmissions = 756000,
                AcceptedSubmissions = 557928
            }
        };

        return Task.FromResult(new ProblemSearchResponseDto
        {
            Problems = mockProblems,
            TotalCount = mockProblems.Count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = 1
        });
    }

    public Task<ProblemDto> GetProblemAsync(string id, string? userId)
    {
        _logger.LogInformation("Mock get problem: {ProblemId}", id);
        
        return Task.FromResult(new ProblemDto
        {
            Id = id,
            Title = "Two Sum",
            Slug = "two-sum", 
            Description = "Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.",
            Difficulty = "Easy",
            Tags = new List<string> { "Array", "Hash Table" },
            SampleInput = "[2,7,11,15], target = 9",
            SampleOutput = "[0,1]",
            AcceptanceRate = 85.2m,
            TotalSubmissions = 1250000,
            AcceptedSubmissions = 1065000,
            CodeTemplate = "class Solution {\n    public int[] TwoSum(int[] nums, int target) {\n        // Your code here\n    }\n}",
            Constraints = new List<string>
            {
                "2 <= nums.length <= 10^4",
                "-10^9 <= nums[i] <= 10^9", 
                "-10^9 <= target <= 10^9",
                "Only one valid answer exists."
            }
        });
    }

    public Task<ProblemDto> GetProblemBySlugAsync(string slug, string? userId)
    {
        _logger.LogInformation("Mock get problem by slug: {Slug}", slug);
        
        return Task.FromResult(new ProblemDto
        {
            Id = "problem-1",
            Title = "Two Sum",
            Slug = slug,
            Description = "Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.",
            Difficulty = "Easy",
            Tags = new List<string> { "Array", "Hash Table" },
            SampleInput = "[2,7,11,15], target = 9",
            SampleOutput = "[0,1]",
            AcceptanceRate = 85.2m,
            TotalSubmissions = 1250000,
            AcceptedSubmissions = 1065000
        });
    }

    public Task<ProblemDto> CreateProblemAsync(CreateProblemRequestDto request, string createdBy)
    {
        _logger.LogInformation("Mock create problem: {Title}", request.Title);
        
        return Task.FromResult(new ProblemDto
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Slug = request.Title.ToLower().Replace(" ", "-"),
            Description = request.Description,
            Difficulty = request.Difficulty,
            Tags = request.Tags ?? new List<string>(),
            SampleInput = request.SampleInput,
            SampleOutput = request.SampleOutput,
            AcceptanceRate = 0,
            TotalSubmissions = 0,
            AcceptedSubmissions = 0
        });
    }

    public Task<ProblemDto> UpdateProblemAsync(string id, UpdateProblemRequestDto request)
    {
        _logger.LogInformation("Mock update problem: {ProblemId}", id);
        
        return Task.FromResult(new ProblemDto
        {
            Id = id,
            Title = request.Title,
            Slug = request.Title?.ToLower().Replace(" ", "-") ?? "updated-problem",
            Description = request.Description,
            Difficulty = request.Difficulty,
            Tags = request.Tags ?? new List<string>(),
            SampleInput = request.SampleInput,
            SampleOutput = request.SampleOutput,
            AcceptanceRate = 75.0m,
            TotalSubmissions = 1000,
            AcceptedSubmissions = 750
        });
    }

    public Task<bool> DeleteProblemAsync(string id)
    {
        _logger.LogInformation("Mock delete problem: {ProblemId}", id);
        return Task.FromResult(true);
    }

    public Task<List<string>> GetPopularTagsAsync(int limit)
    {
        _logger.LogInformation("Mock get popular tags with limit: {Limit}", limit);
        
        return Task.FromResult(new List<string>
        {
            "Array", "Hash Table", "String", "Dynamic Programming", "Math",
            "Tree", "Depth-First Search", "Binary Search", "Two Pointers", "Greedy"
        }.Take(limit).ToList());
    }

    public Task<List<ProblemDto>> GetRandomProblemsAsync(string difficulty, int count)
    {
        _logger.LogInformation("Mock get random problems: {Difficulty}, count: {Count}", difficulty, count);
        
        var mockProblems = new List<ProblemDto>
        {
            new ProblemDto
            {
                Id = "random-1",
                Title = "Valid Parentheses",
                Slug = "valid-parentheses",
                Description = "Given a string s containing just the characters '(', ')', '{', '}', '[' and ']', determine if the input string is valid.",
                Difficulty = difficulty,
                Tags = new List<string> { "String", "Stack" },
                SampleInput = "s = \"()\"",
                SampleOutput = "true"
            }
        };

        return Task.FromResult(mockProblems.Take(count).ToList());
    }
}