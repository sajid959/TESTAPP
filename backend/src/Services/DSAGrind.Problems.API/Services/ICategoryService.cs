using DSAGrind.Models.DTOs;

namespace DSAGrind.Problems.API.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetCategoryAsync(string id, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<CategoryDto?> UpdateCategoryAsync(string id, UpdateCategoryRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(string id, string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateCategoryOrderAsync(List<CategoryOrderDto> orders, string userId, CancellationToken cancellationToken = default);
}

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
    public bool IsPaid { get; set; }
    public int FreeProblemLimit { get; set; } = 5;
    public List<string> Tags { get; set; } = new();
}

public class UpdateCategoryRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool? IsPaid { get; set; }
    public int? FreeProblemLimit { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? Tags { get; set; }
}

public class CategoryOrderDto
{
    public string Id { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}