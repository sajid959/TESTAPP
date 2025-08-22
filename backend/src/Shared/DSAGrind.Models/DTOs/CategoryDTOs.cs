namespace DSAGrind.Models.DTOs;

public class CategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsPaid { get; set; }
    public int FreeProblemLimit { get; set; }
    public int TotalProblems { get; set; }
    public bool IsActive { get; set; }
    public int OrderIndex { get; set; }
    public CategoryMetadataDto Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CategoryMetadataDto
{
    public DifficultyDistributionDto Difficulty { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public int EstimatedTimeHours { get; set; }
}

public class DifficultyDistributionDto
{
    public int Easy { get; set; }
    public int Medium { get; set; }
    public int Hard { get; set; }
}

public class CreateCategoryRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsPaid { get; set; } = false;
    public int FreeProblemLimit { get; set; } = 5;
    public int OrderIndex { get; set; } = 0;
}

public class UpdateCategoryRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool? IsPaid { get; set; }
    public int? FreeProblemLimit { get; set; }
    public int? OrderIndex { get; set; }
    public bool? IsActive { get; set; }
}