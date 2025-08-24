using System.ComponentModel.DataAnnotations;

namespace DSAGrind.Models.DTOs;

public class ProblemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<string> Companies { get; set; } = new();
    public List<TestCaseDto> TestCases { get; set; } = new();
    public ProblemStatsDto Stats { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProblemStatsDto
{
    public int TotalSubmissions { get; set; }
    public int AcceptedSubmissions { get; set; }
    public double AcceptanceRate { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
}

public class CreateProblemRequestDto
{
    [Required]
    public string CategoryId { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Difficulty { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();
    public List<ProblemExampleDto> Examples { get; set; } = new();
    public List<string> Constraints { get; set; } = new();
    public List<TestCaseDto> TestCases { get; set; } = new();
    public List<TestCaseDto> HiddenTestCases { get; set; } = new();
    public bool IsPaid { get; set; } = false;
    public List<string> Hints { get; set; } = new();
    public ProblemSolutionDto? Solution { get; set; }
    public string? SolutionTemplate { get; set; }
}

public class UpdateProblemRequestDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Difficulty { get; set; }
    public List<string>? Tags { get; set; }
    public List<ProblemExampleDto>? Examples { get; set; }
    public List<string>? Constraints { get; set; }
    public List<TestCaseDto>? TestCases { get; set; }
    public List<TestCaseDto>? HiddenTestCases { get; set; }
    public bool? IsPaid { get; set; }
    public List<string>? Hints { get; set; }
    public ProblemSolutionDto? Solution { get; set; }
}

public class ProblemExampleDto
{
    [Required]
    public string Input { get; set; } = string.Empty;

    [Required]
    public string Output { get; set; } = string.Empty;

    public string? Explanation { get; set; }
}

public class TestCaseDto
{
    [Required]
    public string Input { get; set; } = string.Empty;

    [Required]
    public string ExpectedOutput { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;
    public int TimeLimit { get; set; } = 1000;
    public int MemoryLimit { get; set; } = 256;
}

public class ProblemSolutionDto
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Language { get; set; } = string.Empty;

    public string? Explanation { get; set; }
    public string? TimeComplexity { get; set; }
    public string? SpaceComplexity { get; set; }
}

public class ProblemStatisticsDto
{
    public int TotalSubmissions { get; set; }
    public int AcceptedSubmissions { get; set; }
    public double AcceptanceRate { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public double AverageRating { get; set; }
    public int SolvedByUsers { get; set; }
}

public class ProblemMetadataDto
{
    public List<string> Companies { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
    public List<string> RelatedProblems { get; set; } = new();
    public int EstimatedTimeMinutes { get; set; }
}

public class ProblemSearchRequestDto
{
    public string? CategoryId { get; set; }
    public string? Difficulty { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsPaid { get; set; }
    public bool? IsApproved { get; set; } = true;
    public string? Search { get; set; }
    public string? SortBy { get; set; } = "title";
    public string? SortOrder { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProblemSearchResponseDto
{
    public List<ProblemDto> Problems { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class BulkImportRequestDto
{
    [Required]
    public string CategoryId { get; set; } = string.Empty;

    [Required]
    public List<BulkProblemDto> Problems { get; set; } = new();
}

public class BulkProblemDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Difficulty { get; set; } = string.Empty;

    public string? Tags { get; set; } // Comma-separated
    public string? Examples { get; set; } // JSON string
    public string? Constraints { get; set; } // Newline-separated
    public string? TestCases { get; set; } // JSON string
    public string? HiddenTestCases { get; set; } // JSON string
    public bool IsPaid { get; set; } = false;
    public string? Hints { get; set; } // Newline-separated
    public string? Solution { get; set; } // JSON string
}