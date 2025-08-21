namespace DSAGrind.Events;

public class SubmissionCreatedEvent
{
    public string SubmissionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ProblemId { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public class SubmissionExecutedEvent
{
    public string SubmissionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ProblemId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? Runtime { get; set; }
    public int? Memory { get; set; }
    public int TestCasesPassed { get; set; }
    public int TotalTestCases { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

public class ProblemSolvedEvent
{
    public string UserId { get; set; } = string.Empty;
    public string ProblemId { get; set; } = string.Empty;
    public string SubmissionId { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int AttemptsCount { get; set; }
    public DateTime SolvedAt { get; set; } = DateTime.UtcNow;
}

public class CodeExecutionRequestedEvent
{
    public string RequestId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ProblemId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public bool IsSubmission { get; set; } // true for submission, false for test run
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}

public class CodeExecutionCompletedEvent
{
    public string RequestId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> Results { get; set; } = new();
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}