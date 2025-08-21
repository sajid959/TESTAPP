using DSAGrind.Models.DTOs;

namespace DSAGrind.AI.API.Services;

public interface IAIService
{
    Task<string> GenerateHintAsync(string problemId, string userCode, int hintLevel, CancellationToken cancellationToken = default);
    Task<string> ExplainSolutionAsync(string problemId, string code, CancellationToken cancellationToken = default);
    Task<CodeAnalysisDto> AnalyzeCodeAsync(string code, string language, string? problemId = null, CancellationToken cancellationToken = default);
    Task<string> GenerateTestCasesAsync(string problemDescription, int count, CancellationToken cancellationToken = default);
    Task<DifficultyEstimateDto> EstimateDifficultyAsync(string problemDescription, CancellationToken cancellationToken = default);
    Task<string> OptimizeCodeAsync(string code, string language, CancellationToken cancellationToken = default);
    Task<string> GenerateAlternativeSolutionAsync(string problemId, string currentCode, string language, CancellationToken cancellationToken = default);
    Task<string> DebugCodeAsync(string code, string language, string errorMessage, CancellationToken cancellationToken = default);
}

public class CodeAnalysisDto
{
    public string TimeComplexity { get; set; } = string.Empty;
    public string SpaceComplexity { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public List<string> Issues { get; set; } = new();
    public int CodeQualityScore { get; set; }
    public List<string> BestPractices { get; set; } = new();
    public List<string> OptimizationTips { get; set; } = new();
}

public class DifficultyEstimateDto
{
    public string EstimatedDifficulty { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> ReasoningFactors { get; set; } = new();
    public int EstimatedTimeMinutes { get; set; }
    public List<string> RequiredConcepts { get; set; } = new();
}

public class AIHintRequestDto
{
    public string ProblemId { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public int HintLevel { get; set; } = 1;
}

public class CodeAnalysisRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string? ProblemId { get; set; }
}

public class SolutionExplanationRequestDto
{
    public string ProblemId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class TestCaseGenerationRequestDto
{
    public string ProblemDescription { get; set; } = string.Empty;
    public int Count { get; set; } = 5;
}

public class CodeOptimizationRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}

public class CodeDebugRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}