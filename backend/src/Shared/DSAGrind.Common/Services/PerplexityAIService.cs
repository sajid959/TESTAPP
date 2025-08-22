using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DSAGrind.Common.Configuration;

namespace DSAGrind.Common.Services;

public class PerplexityAIService : IAIProviderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PerplexityAIService> _logger;
    private readonly AISettings _aiSettings;

    public PerplexityAIService(HttpClient httpClient, ILogger<PerplexityAIService> logger, IOptions<AISettings> aiSettings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _aiSettings = aiSettings.Value;
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_aiSettings.PerplexityApiKey}");
    }

    public async Task<string> GenerateHintAsync(string problemDescription, string userCode, int hintLevel, CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            You are an expert programming tutor. Generate a helpful hint for this coding problem.
            
            Problem Description: {problemDescription}
            User's current code: {userCode}
            Hint Level: {hintLevel} (1=gentle nudge, 2=more specific, 3=detailed guidance)
            
            Provide a hint that guides the user without giving away the complete solution.
            Focus on the algorithmic approach or key insight needed.
            """;

        return await CallPerplexityAsync(prompt, cancellationToken);
    }

    public async Task<string> ExplainSolutionAsync(string problemDescription, string solutionCode, CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            You are an expert programming educator. Explain this solution to a coding problem in detail.
            
            Problem Description: {problemDescription}
            Solution Code: {solutionCode}
            
            Provide a comprehensive explanation that covers:
            1. The algorithm/approach used
            2. Time and space complexity
            3. Key insights and why this approach works
            4. Line-by-line walkthrough of important parts
            
            Make it educational and easy to understand.
            """;

        return await CallPerplexityAsync(prompt, cancellationToken);
    }

    public async Task<string> AnalyzeCodeAsync(string code, string language, string? problemDescription = null, CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            You are an expert code reviewer. Analyze this {language} code and provide detailed feedback.
            
            Code: {code}
            {(problemDescription != null ? $"Problem Description: {problemDescription}" : "")}
            
            Analyze and provide:
            1. Time complexity (Big O notation)
            2. Space complexity (Big O notation)
            3. Code quality score (1-100)
            4. List of suggestions for improvement
            5. List of issues or potential bugs
            6. Best practices recommendations
            7. Optimization tips
            
            Return your analysis in a structured format.
            """;

        return await CallPerplexityAsync(prompt, cancellationToken);
    }

    public async Task<string> GenerateTestCasesAsync(string problemDescription, int count, CancellationToken cancellationToken = default)
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

        return await CallPerplexityAsync(prompt, cancellationToken);
    }

    public async Task<string> EstimateDifficultyAsync(string problemDescription, CancellationToken cancellationToken = default)
    {
        var prompt = $$$"""
            You are an expert programming problem curator. Analyze this problem and estimate its difficulty.
            
            Problem Description: {{{problemDescription}}}
            
            Provide your analysis in JSON format:
            {{
                "estimatedDifficulty": "Easy|Medium|Hard",
                "confidence": 0.85,
                "reasoningFactors": ["factor1", "factor2"],
                "estimatedTimeMinutes": 30,
                "requiredConcepts": ["concept1", "concept2"]
            }}
            
            Consider:
            - Algorithmic complexity required
            - Data structures needed
            - Problem-solving techniques
            - Implementation difficulty
            - Typical time to solve for different skill levels
            """;

        return await CallPerplexityAsync(prompt, cancellationToken);
    }

    public async Task<string> OptimizeCodeAsync(string code, string language, CancellationToken cancellationToken = default)
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

        return await CallPerplexityAsync(prompt, cancellationToken);
    }

    public async Task<string> DebugCodeAsync(string code, string language, string errorMessage, CancellationToken cancellationToken = default)
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

        return await CallPerplexityAsync(prompt, cancellationToken);
    }

    public async Task<string> GenerateAlternativeSolutionAsync(string problemDescription, string existingCode, string language, CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            You are an expert programmer. Generate an alternative solution to this problem using a different approach.
            
            Problem Description: {problemDescription}
            Existing Solution: {existingCode}
            Language: {language}
            
            Provide:
            1. A different algorithmic approach
            2. Complete alternative solution
            3. Comparison of time/space complexity
            4. Explanation of when to use each approach
            
            Make sure the alternative uses a genuinely different algorithm or technique.
            """;

        return await CallPerplexityAsync(prompt, cancellationToken);
    }

    private async Task<string> CallPerplexityAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestData = new
            {
                model = _aiSettings.PerplexityModel,
                messages = new[]
                {
                    new { role = "system", content = "Be precise, concise, and educational in your responses." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.2,
                top_p = 0.9,
                max_tokens = _aiSettings.MaxTokens,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.perplexity.ai/chat/completions", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Perplexity API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return "I'm unable to process your request at the moment. Please try again later.";
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
            
            return responseObj.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() 
                   ?? "Unable to generate response.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Perplexity API");
            return "I'm experiencing technical difficulties. Please try again later.";
        }
    }
}