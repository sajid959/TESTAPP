namespace DSAGrind.Events;

public class ProblemCreatedEvent
{
    public string ProblemId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public bool IsPaid { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ProblemUpdatedEvent
{
    public string ProblemId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public Dictionary<string, object> Changes { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ProblemApprovedEvent
{
    public string ProblemId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
}

public class ProblemDeletedEvent
{
    public string ProblemId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
}

public class ProblemViewedEvent
{
    public string ProblemId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}

public class ProblemLikedEvent
{
    public string ProblemId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool IsLike { get; set; } // true for like, false for dislike
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
}

public class BulkProblemsImportedEvent
{
    public string CategoryId { get; set; } = string.Empty;
    public List<string> ProblemIds { get; set; } = new();
    public int TotalImported { get; set; }
    public int TotalFailed { get; set; }
    public string ImportedBy { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}