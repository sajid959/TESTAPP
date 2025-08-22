using DSAGrind.Models.DTOs;

namespace DSAGrind.Submissions.API.Services;

public interface ICodeExecutionService
{
    Task<CodeExecutionResultDto> ExecuteCodeAsync(string problemId, string code, string language, CancellationToken cancellationToken = default);
    Task<CodeExecutionResultDto> TestCodeAsync(string code, string language, string input, CancellationToken cancellationToken = default);
    Task<List<string>> GetSupportedLanguagesAsync(CancellationToken cancellationToken = default);
}