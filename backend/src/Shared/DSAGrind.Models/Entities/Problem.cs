using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DSAGrind.Models.Entities;

public class Problem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("categoryId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("difficulty")]
    public string Difficulty { get; set; } = string.Empty; // Easy, Medium, Hard

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("examples")]
    public List<ProblemExample> Examples { get; set; } = new();

    [BsonElement("constraints")]
    public List<string> Constraints { get; set; } = new();

    [BsonElement("testCases")]
    public List<TestCase> TestCases { get; set; } = new();

    [BsonElement("hiddenTestCases")]
    public List<TestCase> HiddenTestCases { get; set; } = new();

    [BsonElement("isPaid")]
    public bool IsPaid { get; set; } = false;

    [BsonElement("isApproved")]
    public bool IsApproved { get; set; } = false;

    [BsonElement("status")]
    public string Status { get; set; } = "draft"; // draft, published, archived

    [BsonElement("orderIndex")]
    public int OrderIndex { get; set; } = 0;

    [BsonElement("solutionTemplate")]
    public string? SolutionTemplate { get; set; }

    [BsonElement("aiEstimatedDifficulty")]
    public string? AiEstimatedDifficulty { get; set; }

    [BsonElement("hints")]
    public List<string> Hints { get; set; } = new();

    [BsonElement("solution")]
    public ProblemSolution? Solution { get; set; }

    [BsonElement("statistics")]
    public ProblemStatistics Statistics { get; set; } = new();

    [BsonElement("metadata")]
    public ProblemMetadata Metadata { get; set; } = new();

    [BsonElement("createdBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? CreatedBy { get; set; }

    [BsonElement("updatedBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UpdatedBy { get; set; }

    [BsonElement("approvedBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ApprovedBy { get; set; }

    [BsonElement("approvedAt")]
    public DateTime? ApprovedAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ProblemExample
{
    [BsonElement("input")]
    public string Input { get; set; } = string.Empty;

    [BsonElement("output")]
    public string Output { get; set; } = string.Empty;

    [BsonElement("explanation")]
    public string? Explanation { get; set; }
}

public class TestCase
{
    [BsonElement("input")]
    public string Input { get; set; } = string.Empty;

    [BsonElement("expectedOutput")]
    public string ExpectedOutput { get; set; } = string.Empty;

    [BsonElement("isVisible")]
    public bool IsVisible { get; set; } = true;

    [BsonElement("timeLimit")]
    public int TimeLimit { get; set; } = 1000; // milliseconds

    [BsonElement("memoryLimit")]
    public int MemoryLimit { get; set; } = 256; // MB
}

public class ProblemSolution
{
    [BsonElement("code")]
    public string Code { get; set; } = string.Empty;

    [BsonElement("language")]
    public string Language { get; set; } = "python";

    [BsonElement("explanation")]
    public string? Explanation { get; set; }

    [BsonElement("timeComplexity")]
    public string? TimeComplexity { get; set; }

    [BsonElement("spaceComplexity")]
    public string? SpaceComplexity { get; set; }
}

public class ProblemStatistics
{
    [BsonElement("totalSubmissions")]
    public int TotalSubmissions { get; set; } = 0;

    [BsonElement("acceptedSubmissions")]
    public int AcceptedSubmissions { get; set; } = 0;

    [BsonElement("acceptanceRate")]
    public double AcceptanceRate { get; set; } = 0.0;

    [BsonElement("likes")]
    public int Likes { get; set; } = 0;

    [BsonElement("dislikes")]
    public int Dislikes { get; set; } = 0;

    [BsonElement("averageRating")]
    public double AverageRating { get; set; } = 0.0;

    [BsonElement("solvedByUsers")]
    public int SolvedByUsers { get; set; } = 0;
}

public class ProblemMetadata
{
    [BsonElement("companies")]
    public List<string> Companies { get; set; } = new();

    [BsonElement("patterns")]
    public List<string> Patterns { get; set; } = new();

    [BsonElement("relatedProblems")]
    public List<string> RelatedProblems { get; set; } = new();

    [BsonElement("estimatedTimeMinutes")]
    public int EstimatedTimeMinutes { get; set; } = 30;

    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}