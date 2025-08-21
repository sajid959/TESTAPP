using DSAGrind.Models.DTOs;

namespace DSAGrind.Submissions.API.Services;

public interface ISubmissionService
{
    Task<SubmissionDto?> GetSubmissionAsync(string id, string userId, CancellationToken cancellationToken = default);
    Task<List<SubmissionDto>> GetUserSubmissionsAsync(string userId, string? problemId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<SubmissionDto> CreateSubmissionAsync(CreateSubmissionRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<CodeExecutionResultDto> ExecuteCodeAsync(CodeExecutionRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<CodeExecutionResultDto> TestCodeAsync(CodeTestRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<List<string>> GetSupportedLanguagesAsync(CancellationToken cancellationToken = default);
    Task<List<SubmissionDto>> GetLeaderboardAsync(string problemId, int limit = 10, CancellationToken cancellationToken = default);
}

public class SubmissionDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ProblemId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? Runtime { get; set; }
    public int? Memory { get; set; }
    public List<TestResultDto> TestResults { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public ExecutionDetailsDto ExecutionDetails { get; set; } = new();
    public bool IsPublic { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TestResultDto
{
    public int TestCaseIndex { get; set; }
    public bool Passed { get; set; }
    public string Input { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public string ActualOutput { get; set; } = string.Empty;
    public int Runtime { get; set; }
    public int Memory { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ExecutionDetailsDto
{
    public int CompilationTime { get; set; }
    public int TotalRuntime { get; set; }
    public int PeakMemoryUsage { get; set; }
    public string ExecutorVersion { get; set; } = string.Empty;
    public SandboxInfoDto SandboxInfo { get; set; } = new();
}

public class SandboxInfoDto
{
    public string ContainerId { get; set; } = string.Empty;
    public DateTime ExecutionStartTime { get; set; }
    public DateTime ExecutionEndTime { get; set; }
    public ResourceLimitsDto ResourceLimits { get; set; } = new();
}

public class ResourceLimitsDto
{
    public int TimeLimit { get; set; } = 1000;
    public int MemoryLimit { get; set; } = 256;
    public double CpuLimit { get; set; } = 1.0;
}

public class CreateSubmissionRequestDto
{
    public string ProblemId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsPublic { get; set; } = false;
}

public class CodeExecutionRequestDto
{
    public string ProblemId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}

public class CodeTestRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
}

public class CodeExecutionResultDto
{
    public string Status { get; set; } = string.Empty;
    public string? Output { get; set; }
    public string? ErrorMessage { get; set; }
    public int Runtime { get; set; }
    public int Memory { get; set; }
    public List<TestResultDto> TestResults { get; set; } = new();
    public ExecutionDetailsDto ExecutionDetails { get; set; } = new();
}