using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DSAGrind.Models.Entities;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // submission_result, contest_reminder, payment_success, etc.

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("payload")]
    public Dictionary<string, object> Payload { get; set; } = new();

    [BsonElement("isRead")]
    public bool IsRead { get; set; } = false;

    [BsonElement("priority")]
    public string Priority { get; set; } = "normal"; // low, normal, high, critical

    [BsonElement("channels")]
    public List<string> Channels { get; set; } = new(); // email, push, in-app

    [BsonElement("sentAt")]
    public DateTime? SentAt { get; set; }

    [BsonElement("readAt")]
    public DateTime? ReadAt { get; set; }

    [BsonElement("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Contest
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("startTime")]
    public DateTime StartTime { get; set; }

    [BsonElement("endTime")]
    public DateTime EndTime { get; set; }

    [BsonElement("problemIds")]
    public List<string> ProblemIds { get; set; } = new();

    [BsonElement("participants")]
    public List<ContestParticipant> Participants { get; set; } = new();

    [BsonElement("leaderboard")]
    public List<LeaderboardEntry> Leaderboard { get; set; } = new();

    [BsonElement("prizes")]
    public List<ContestPrize> Prizes { get; set; } = new();

    [BsonElement("rules")]
    public ContestRules Rules { get; set; } = new();

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("isPublic")]
    public bool IsPublic { get; set; } = true;

    [BsonElement("createdBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ContestParticipant
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("score")]
    public int Score { get; set; } = 0;

    [BsonElement("rank")]
    public int Rank { get; set; } = 0;

    [BsonElement("submissions")]
    public List<ContestSubmission> Submissions { get; set; } = new();

    [BsonElement("joinedAt")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class ContestSubmission
{
    [BsonElement("problemId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProblemId { get; set; } = string.Empty;

    [BsonElement("submissionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SubmissionId { get; set; } = string.Empty;

    [BsonElement("score")]
    public int Score { get; set; } = 0;

    [BsonElement("penalty")]
    public int Penalty { get; set; } = 0;

    [BsonElement("submittedAt")]
    public DateTime SubmittedAt { get; set; }
}

public class LeaderboardEntry
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("rank")]
    public int Rank { get; set; }

    [BsonElement("score")]
    public int Score { get; set; }

    [BsonElement("penalty")]
    public int Penalty { get; set; }

    [BsonElement("problemsSolved")]
    public int ProblemsSolved { get; set; }
}

public class ContestPrize
{
    [BsonElement("position")]
    public int Position { get; set; }

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("value")]
    public decimal? Value { get; set; }

    [BsonElement("currency")]
    public string? Currency { get; set; }
}

public class ContestRules
{
    [BsonElement("scoringSystem")]
    public string ScoringSystem { get; set; } = "standard"; // standard, icpc, ioi

    [BsonElement("penaltyPerWrongSubmission")]
    public int PenaltyPerWrongSubmission { get; set; } = 20;

    [BsonElement("maxSubmissionsPerProblem")]
    public int? MaxSubmissionsPerProblem { get; set; }

    [BsonElement("allowedLanguages")]
    public List<string> AllowedLanguages { get; set; } = new();

    [BsonElement("freezeTime")]
    public int? FreezeTime { get; set; } // minutes before end when leaderboard freezes
}