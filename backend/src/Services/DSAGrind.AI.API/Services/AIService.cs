using DSAGrind.Models.DTOs;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

namespace DSAGrind.AI.API.Services;

public class AIService : IAIService
{
    private readonly Kernel _kernel;
    private readonly ILogger<AIService> _logger;

    public AIService(IConfiguration configuration, ILogger<AIService> logger)
    {
        _logger = logger;
        
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            "gpt-3.5-turbo",
            configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured"));
        
        _kernel = builder.Build();
    }

    public async Task<string> GenerateHintAsync(string problemId, string userCode, int hintLevel, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"""
                You are an expert programming tutor. Generate a helpful hint for this coding problem.
                
                Problem ID: {problemId}
                User's current code: {userCode}
                Hint Level: {hintLevel} (1=gentle nudge, 2=more specific, 3=detailed guidance)
                
                Provide a hint that guides the user without giving away the complete solution.
                Focus on the algorithmic approach or key insight needed.
                """;

            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating hint for problem {ProblemId}", problemId);
            return "Unable to generate hint at this time. Please try again later.";
        }
    }

    public async Task<string> ExplainSolutionAsync(string problemId, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"""
                You are an expert programming educator. Explain this solution to a coding problem in detail.
                
                Problem ID: {problemId}
                Solution Code: {code}
                
                Provide a comprehensive explanation that covers:
                1. The algorithm/approach used
                2. Time and space complexity
                3. Key insights and why this approach works
                4. Line-by-line walkthrough of important parts
                
                Make it educational and easy to understand.
                """;

            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining solution for problem {ProblemId}", problemId);
            return "Unable to explain solution at this time. Please try again later.";
        }
    }

    public async Task<CodeAnalysisDto> AnalyzeCodeAsync(string code, string language, string? problemId = null, CancellationToken cancellationToken = default)
    {
        try
        {


            var prompt = $$"""
You are an expert programming problem curator. Analyze this problem and estimate its difficulty.

Problem Description: {problemDescription}

Provide your analysis in this JSON format:
{
    "estimatedDifficulty": "Easy|Medium|Hard",
    "confidence": 0.85,
    "reasoningFactors": ["factor1", "factor2"],
    "estimatedTimeMinutes": 30,
    "requiredConcepts": ["concept1", "concept2"]
}

Consider:
- Algorithmic complexity required
- Data structures needed
- Problem-solving techniques
- Implementation difficulty
- Typical time to solve for different skill levels
""";


            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            
            // Parse the JSON response
            var response = result.ToString();
            var analysis = System.Text.Json.JsonSerializer.Deserialize<CodeAnalysisDto>(response, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return analysis ?? new CodeAnalysisDto
            {
                TimeComplexity = "Unable to analyze",
                SpaceComplexity = "Unable to analyze",
                CodeQualityScore = 50,
                Suggestions = new List<string> { "Analysis failed, please try again" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing code");
            return new CodeAnalysisDto
            {
                TimeComplexity = "Analysis failed",
                SpaceComplexity = "Analysis failed",
                CodeQualityScore = 0,
                Issues = new List<string> { "Unable to analyze code at this time" }
            };
        }
    }

    public async Task<string> GenerateTestCasesAsync(string problemDescription, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"""
                You are an expert test case designer. Generate {count} diverse test cases for this problem.
                
                Problem Description: {problemDescription}
                
                Generate test cases that cover:
                - Edge cases (empty input, single element, etc.)
                - Boundary conditions
                - Normal cases
                - Large inputs (if applicable)
                
                Format as:
                Input: [input]
                Expected Output: [output]
                ---
                (repeat for each test case)
                """;

            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating test cases");
            return "Unable to generate test cases at this time.";
        }
    }

    public async Task<DifficultyEstimateDto> EstimateDifficultyAsync(string problemDescription, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = @"
You are an expert programming problem curator. Analyze this problem and estimate its difficulty.

Problem Description: {problemDescription}

Provide your analysis in this JSON format:
{
    ""estimatedDifficulty"": ""Easy|Medium|Hard"",
    ""confidence"": 0.85,
    ""reasoningFactors"": [""factor1"", ""factor2""],
    ""estimatedTimeMinutes"": 30,
    ""requiredConcepts"": [""concept1"", ""concept2""]
}

Consider:
- Algorithmic complexity required
- Data structures needed
- Problem-solving techniques
- Implementation difficulty
- Typical time to solve for different skill levels
";

            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            
            var response = result.ToString();
            var estimate = System.Text.Json.JsonSerializer.Deserialize<DifficultyEstimateDto>(response, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return estimate ?? new DifficultyEstimateDto
            {
                EstimatedDifficulty = "Medium",
                Confidence = 0.5,
                ReasoningFactors = new List<string> { "Analysis incomplete" },
                EstimatedTimeMinutes = 45,
                RequiredConcepts = new List<string> { "Problem solving" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating difficulty");
            return new DifficultyEstimateDto
            {
                EstimatedDifficulty = "Unknown",
                Confidence = 0.0,
                ReasoningFactors = new List<string> { "Analysis failed" },
                EstimatedTimeMinutes = 0,
                RequiredConcepts = new List<string>()
            };
        }
    }

    public async Task<string> OptimizeCodeAsync(string code, string language, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"""
                You are an expert code optimizer. Optimize this {language} code for better performance and readability.
                
                Original Code: {code}
                
                Provide an optimized version that:
                1. Improves time/space complexity if possible
                2. Follows best practices
                3. Is more readable and maintainable
                4. Includes brief comments explaining optimizations
                
                Return only the optimized code.
                """;

            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing code");
            return "Unable to optimize code at this time. Please try again later.";
        }
    }

    public async Task<string> GenerateAlternativeSolutionAsync(string problemId, string currentCode, string language, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"""
                You are an expert programmer. Generate an alternative solution approach for this problem.
                
                Problem ID: {problemId}
                Current Solution: {currentCode}
                Language: {language}
                
                Provide a completely different algorithmic approach that:
                1. Uses different data structures or algorithms
                2. Has different time/space complexity trade-offs
                3. Demonstrates alternative problem-solving techniques
                4. Is well-commented to explain the approach
                
                Return only the alternative solution code.
                """;

            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating alternative solution");
            return "Unable to generate alternative solution at this time.";
        }
    }

    public async Task<string> DebugCodeAsync(string code, string language, string errorMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"""
                You are an expert debugger. Help fix this {language} code that's producing an error.
                
                Code: {code}
                Error Message: {errorMessage}
                
                Provide:
                1. Explanation of what's causing the error
                2. The corrected code
                3. Explanation of the fix
                4. Tips to avoid similar errors in the future
                
                Be clear and educational in your response.
                """;

            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error debugging code");
            return "Unable to debug code at this time. Please try again later.";
        }
    }
}