using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DSAGrind.Models.Entities;

public class Submission
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("problemId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProblemId { get; set; } = string.Empty;

    [BsonElement("code")]
    public string Code { get; set; } = string.Empty;

    [BsonElement("language")]
    public string Language { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty; // Accepted, Wrong Answer, Time Limit Exceeded, etc.

    [BsonElement("runtime")]
    public int? Runtime { get; set; } // milliseconds

    [BsonElement("memory")]
    public int? Memory { get; set; } // KB

    [BsonElement("testResults")]
    public List<TestResult> TestResults { get; set; } = new();

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    [BsonElement("executionDetails")]
    public ExecutionDetails ExecutionDetails { get; set; } = new();

    [BsonElement("isPublic")]
    public bool IsPublic { get; set; } = false;

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TestResult
{
    [BsonElement("testCaseIndex")]
    public int TestCaseIndex { get; set; }

    [BsonElement("passed")]
    public bool Passed { get; set; }

    [BsonElement("input")]
    public string Input { get; set; } = string.Empty;

    [BsonElement("expectedOutput")]
    public string ExpectedOutput { get; set; } = string.Empty;

    [BsonElement("actualOutput")]
    public string ActualOutput { get; set; } = string.Empty;

    [BsonElement("runtime")]
    public int Runtime { get; set; } // milliseconds

    [BsonElement("memory")]
    public int Memory { get; set; } // KB

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }
}

public class ExecutionDetails
{
    [BsonElement("compilationTime")]
    public int CompilationTime { get; set; } = 0;

    [BsonElement("totalRuntime")]
    public int TotalRuntime { get; set; } = 0;

    [BsonElement("peakMemoryUsage")]
    public int PeakMemoryUsage { get; set; } = 0;

    [BsonElement("executorVersion")]
    public string ExecutorVersion { get; set; } = string.Empty;

    [BsonElement("sandboxInfo")]
    public SandboxInfo SandboxInfo { get; set; } = new();
}

public class SandboxInfo
{
    [BsonElement("containerId")]
    public string ContainerId { get; set; } = string.Empty;

    [BsonElement("executionStartTime")]
    public DateTime ExecutionStartTime { get; set; }

    [BsonElement("executionEndTime")]
    public DateTime ExecutionEndTime { get; set; }

    [BsonElement("resourceLimits")]
    public ResourceLimits ResourceLimits { get; set; } = new();
}

public class ResourceLimits
{
    [BsonElement("timeLimit")]
    public int TimeLimit { get; set; } = 1000; // milliseconds

    [BsonElement("memoryLimit")]
    public int MemoryLimit { get; set; } = 256; // MB

    [BsonElement("cpuLimit")]
    public double CpuLimit { get; set; } = 1.0; // CPU cores
}