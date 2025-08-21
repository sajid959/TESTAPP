using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DSAGrind.Models.Entities;

public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("icon")]
    public string? Icon { get; set; }

    [BsonElement("isPaid")]
    public bool IsPaid { get; set; } = false;

    [BsonElement("freeProblemLimit")]
    public int FreeProblemLimit { get; set; } = 5;

    [BsonElement("totalProblems")]
    public int TotalProblems { get; set; } = 0;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("orderIndex")]
    public int OrderIndex { get; set; } = 0;

    [BsonElement("metadata")]
    public CategoryMetadata Metadata { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class CategoryMetadata
{
    [BsonElement("difficulty")]
    public DifficultyDistribution Difficulty { get; set; } = new();

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("estimatedTimeHours")]
    public int EstimatedTimeHours { get; set; } = 0;
}

public class DifficultyDistribution
{
    [BsonElement("easy")]
    public int Easy { get; set; } = 0;

    [BsonElement("medium")]
    public int Medium { get; set; } = 0;

    [BsonElement("hard")]
    public int Hard { get; set; } = 0;
}