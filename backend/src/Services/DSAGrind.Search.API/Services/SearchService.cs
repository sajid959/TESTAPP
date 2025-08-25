using DSAGrind.Common.Services;
using DSAGrind.Models.DTOs;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace DSAGrind.Search.API.Services;

public class SearchService : ISearchService
{
    private readonly QdrantClient _qdrantClient;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SearchService> _logger;

    public SearchService(
        QdrantClient qdrantClient,
        IVectorSearchService vectorSearchService,
        IEventPublisher eventPublisher,
        ILogger<SearchService> logger)
    {
        _qdrantClient = qdrantClient;
        _vectorSearchService = vectorSearchService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<SearchResultDto> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;

            // Perform vector search for semantic similarity
            var vectorResults = await _vectorSearchService.SearchProblemsAsync(request.Query, request.PageSize, cancellationToken);

            // Apply additional filters
            var filteredResults = ApplyFilters(vectorResults, request);

            var totalCount = filteredResults.Count;
            var pagedResults = filteredResults
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var searchTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Track search analytics
            await TrackSearchAsync(request.Query, totalCount, searchTime, cancellationToken);

            return new SearchResultDto
            {
                Problems = pagedResults,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.Page * request.PageSize < totalCount,
                HasPreviousPage = request.Page > 1,
                Suggestions = await GetSearchSuggestionsAsync(request.Query, 3, cancellationToken),
                Facets = BuildFacets(filteredResults),
                SearchTime = searchTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search for query: {Query}", request.Query);
            return new SearchResultDto
            {
                Problems = new List<ProblemDto>(),
                TotalCount = 0,
                Page = request.Page,
                PageSize = request.PageSize,
                SearchTime = 0
            };
        }
    }

    public async Task<List<ProblemDto>> GetSimilarProblemsAsync(string problemId, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _vectorSearchService.GetSimilarProblemsAsync(problemId, count, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar problems for {ProblemId}", problemId);
            return new List<ProblemDto>();
        }
    }

    public async Task<List<ProblemDto>> GetRecommendationsAsync(string userId, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would use user's solving history, preferences, etc.
            return await _vectorSearchService.GetPersonalizedRecommendationsAsync(userId, count, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations for user {UserId}", userId);
            return new List<ProblemDto>();
        }
    }

    public async Task<bool> IndexProblemAsync(string problemId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _vectorSearchService.IndexProblemAsync(problemId, cancellationToken);
            await _eventPublisher.PublishAsync("problem.indexed", new { ProblemId = problemId }, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing problem {ProblemId}", problemId);
            return false;
        }
    }

    public async Task<bool> UpdateProblemIndexAsync(string problemId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _vectorSearchService.UpdateProblemIndexAsync(problemId, cancellationToken);
            await _eventPublisher.PublishAsync("problem.index_updated", new { ProblemId = problemId }, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating problem index {ProblemId}", problemId);
            return false;
        }
    }

    public async Task<bool> DeleteProblemIndexAsync(string problemId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _vectorSearchService.DeleteProblemIndexAsync(problemId, cancellationToken);
            await _eventPublisher.PublishAsync("problem.index_deleted", new { ProblemId = problemId }, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting problem index {ProblemId}", problemId);
            return false;
        }
    }

    public async Task<List<string>> GetSearchSuggestionsAsync(string query, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate suggestions based on popular searches and query similarity
            var suggestions = new List<string>();

            // Add common programming terms that match the query
            var commonTerms = new[]
            {
                "array", "linked list", "binary tree", "graph", "dynamic programming",
                "sorting", "searching", "hash table", "queue", "stack", "heap",
                "two pointers", "sliding window", "backtracking", "greedy",
                "binary search", "depth-first search", "breadth-first search"
            };

            suggestions.AddRange(commonTerms
                .Where(term => term.Contains(query.ToLower(), StringComparison.OrdinalIgnoreCase))
                .Take(count));

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating search suggestions");
            return new List<string>();
        }
    }

    public async Task<List<ProblemDto>> SearchByTagsAsync(List<string> tags, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _vectorSearchService.SearchByTagsAsync(tags, count, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching by tags");
            return new List<ProblemDto>();
        }
    }

    public async Task<SearchAnalyticsDto> GetSearchAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would query analytics database
            return new SearchAnalyticsDto
            {
                PopularSearches = new List<PopularSearchDto>
                {
                    new() { Query = "binary tree", Count = 1523, LastSearched = DateTime.UtcNow.AddHours(-1) },
                    new() { Query = "dynamic programming", Count = 1245, LastSearched = DateTime.UtcNow.AddHours(-2) },
                    new() { Query = "graph algorithms", Count = 987, LastSearched = DateTime.UtcNow.AddHours(-3) }
                },
                TrendingTags = new List<string> { "array", "tree", "dp", "graph", "sorting" },
                SearchFrequency = new Dictionary<string, int>
                {
                    ["today"] = 5420,
                    ["yesterday"] = 4890,
                    ["this_week"] = 32100
                },
                AverageSearchTime = 145.2
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search analytics");
            return new SearchAnalyticsDto();
        }
    }

    private List<ProblemDto> ApplyFilters(List<ProblemDto> problems, SearchRequestDto request)
    {
        var query = problems.AsQueryable();

        if (!string.IsNullOrEmpty(request.Difficulty))
        {
            query = query.Where(p => p.Difficulty.Equals(request.Difficulty, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(p => p.CategoryId == request.Category);
        }

        if (request.IsPaid.HasValue)
        {
            query = query.Where(p => p.IsPaid == request.IsPaid.Value);
        }

        if (request.Tags?.Any() == true)
        {
            query = query.Where(p => p.Tags.Any(tag => request.Tags.Contains(tag)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "difficulty" => request.SortOrder == "desc" 
                ? query.OrderByDescending(p => p.Difficulty) 
                : query.OrderBy(p => p.Difficulty),
            "title" => request.SortOrder == "desc" 
                ? query.OrderByDescending(p => p.Title) 
                : query.OrderBy(p => p.Title),
            "createdat" => request.SortOrder == "desc" 
                ? query.OrderByDescending(p => p.CreatedAt) 
                : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.OrderIndex)
        };

        return query.ToList();
    }

    private Dictionary<string, int> BuildFacets(List<ProblemDto> problems)
    {
        return new Dictionary<string, int>
        {
            ["Easy"] = problems.Count(p => p.Difficulty == "Easy"),
            ["Medium"] = problems.Count(p => p.Difficulty == "Medium"),
            ["Hard"] = problems.Count(p => p.Difficulty == "Hard"),
            ["Free"] = problems.Count(p => !p.IsPaid),
            ["Premium"] = problems.Count(p => p.IsPaid)
        };
    }

    private async Task TrackSearchAsync(string query, int resultCount, double searchTime, CancellationToken cancellationToken)
    {
        try
        {
            await _eventPublisher.PublishAsync("search.performed", new 
            { 
                Query = query, 
                ResultCount = resultCount, 
                SearchTime = searchTime,
                Timestamp = DateTime.UtcNow 
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking search analytics");
        }
    }
}

