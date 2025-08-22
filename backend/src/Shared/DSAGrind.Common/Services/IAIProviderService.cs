namespace DSAGrind.Common.Services;

public interface IAIProviderService
{
    Task<string> GenerateHintAsync(string problemDescription, string userCode, int hintLevel, CancellationToken cancellationToken = default);
    Task<string> ExplainSolutionAsync(string problemDescription, string solutionCode, CancellationToken cancellationToken = default);
    Task<string> AnalyzeCodeAsync(string code, string language, string? problemDescription = null, CancellationToken cancellationToken = default);
    Task<string> GenerateTestCasesAsync(string problemDescription, int count, CancellationToken cancellationToken = default);
    Task<string> EstimateDifficultyAsync(string problemDescription, CancellationToken cancellationToken = default);
    Task<string> OptimizeCodeAsync(string code, string language, CancellationToken cancellationToken = default);
    Task<string> DebugCodeAsync(string code, string language, string errorMessage, CancellationToken cancellationToken = default);
    Task<string> GenerateAlternativeSolutionAsync(string problemDescription, string existingCode, string language, CancellationToken cancellationToken = default);
}

public enum AIProvider
{
    OpenAI,
    Perplexity,
    Anthropic,
    Google,
    Local
}