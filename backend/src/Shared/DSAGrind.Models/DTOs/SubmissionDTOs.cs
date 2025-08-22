using System.ComponentModel.DataAnnotations;

namespace DSAGrind.Models.DTOs;

public class SubmissionDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ProblemId { get; set; } = string.Empty;
    public string ProblemTitle { get; set; } = string.Empty;
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

public class CreateSubmissionRequestDto
{
    [Required]
    public string ProblemId { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Language { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = false;
    public string? Notes { get; set; }
}

public class CodeExecutionRequestDto
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Language { get; set; } = string.Empty;

    [Required]
    public string Input { get; set; } = string.Empty;

    public int TimeLimit { get; set; } = 5000; // milliseconds
    public int MemoryLimit { get; set; } = 256; // MB
}

public class CodeTestRequestDto
{
    [Required]
    public string ProblemId { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Language { get; set; } = string.Empty;

    public bool RunVisibleTestsOnly { get; set; } = true;
}

public class CodeExecutionResultDto
{
    public string Status { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int Runtime { get; set; }
    public int Memory { get; set; }
    public List<TestResultDto> TestResults { get; set; } = new();
    public ExecutionDetailsDto ExecutionDetails { get; set; } = new();
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

public class SubmissionSearchRequestDto
{
    public string? ProblemId { get; set; }
    public string? UserId { get; set; }
    public string? Status { get; set; }
    public string? Language { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SortBy { get; set; } = "createdAt";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SubmissionSearchResponseDto
{
    public List<SubmissionDto> Submissions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}