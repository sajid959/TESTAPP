using DSAGrind.Models.DTOs;

namespace DSAGrind.Search.API.Services;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
    Task<List<ProblemDto>> GetSimilarProblemsAsync(string problemId, int count, CancellationToken cancellationToken = default);
    Task<List<ProblemDto>> GetRecommendationsAsync(string userId, int count, CancellationToken cancellationToken = default);
    Task<bool> IndexProblemAsync(string problemId, CancellationToken cancellationToken = default);
    Task<bool> UpdateProblemIndexAsync(string problemId, CancellationToken cancellationToken = default);
    Task<bool> DeleteProblemIndexAsync(string problemId, CancellationToken cancellationToken = default);
    Task<List<string>> GetSearchSuggestionsAsync(string query, int count, CancellationToken cancellationToken = default);
    Task<List<ProblemDto>> SearchByTagsAsync(List<string> tags, int count, CancellationToken cancellationToken = default);
    Task<SearchAnalyticsDto> GetSearchAnalyticsAsync(CancellationToken cancellationToken = default);
}

public class SearchRequestDto
{
    public string Query { get; set; } = string.Empty;
    public List<string>? Tags { get; set; }
    public string? Difficulty { get; set; }
    public string? Category { get; set; }
    public bool? IsPaid { get; set; }
    public string? SortBy { get; set; } = "relevance";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchResultDto
{
    public List<ProblemDto> Problems { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public List<string> Suggestions { get; set; } = new();
    public Dictionary<string, int> Facets { get; set; } = new();
    public double SearchTime { get; set; }
}

public class SearchAnalyticsDto
{
    public List<PopularSearchDto> PopularSearches { get; set; } = new();
    public List<string> TrendingTags { get; set; } = new();
    public Dictionary<string, int> SearchFrequency { get; set; } = new();
    public double AverageSearchTime { get; set; }
}

public class PopularSearchDto
{
    public string Query { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime LastSearched { get; set; }
}