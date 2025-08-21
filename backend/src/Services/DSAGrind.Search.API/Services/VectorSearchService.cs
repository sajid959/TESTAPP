using DSAGrind.Models.DTOs;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace DSAGrind.Search.API.Services;

public class VectorSearchService : IVectorSearchService
{
    private readonly QdrantClient _qdrantClient;
    private readonly ILogger<VectorSearchService> _logger;
    private const string COLLECTION_NAME = "problems";

    public VectorSearchService(QdrantClient qdrantClient, ILogger<VectorSearchService> logger)
    {
        _qdrantClient = qdrantClient;
        _logger = logger;
    }

    public async Task<List<ProblemDto>> SearchProblemsAsync(string query, int limit, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would:
            // 1. Generate embeddings for the query using an embedding model
            // 2. Perform vector search in Qdrant
            // 3. Return the most similar problems

            // For now, return sample problems
            return await Task.FromResult(GetSampleProblems().Take(limit).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing vector search for query: {Query}", query);
            return new List<ProblemDto>();
        }
    }

    public async Task<List<ProblemDto>> GetSimilarProblemsAsync(string problemId, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would:
            // 1. Get the vector for the given problem
            // 2. Search for similar vectors
            // 3. Return similar problems

            return await Task.FromResult(GetSampleProblems().Take(count).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar problems for {ProblemId}", problemId);
            return new List<ProblemDto>();
        }
    }

    public async Task<List<ProblemDto>> GetPersonalizedRecommendationsAsync(string userId, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would:
            // 1. Analyze user's solving history
            // 2. Find patterns in their preferences
            // 3. Recommend similar problems they haven't solved

            return await Task.FromResult(GetSampleProblems().Take(count).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personalized recommendations for user {UserId}", userId);
            return new List<ProblemDto>();
        }
    }

    public async Task<List<ProblemDto>> SearchByTagsAsync(List<string> tags, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            // Filter sample problems by tags
            var filteredProblems = GetSampleProblems()
                .Where(p => p.Tags.Any(tag => tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                .Take(count)
                .ToList();

            return await Task.FromResult(filteredProblems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching by tags");
            return new List<ProblemDto>();
        }
    }

    public async Task IndexProblemAsync(string problemId, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would:
            // 1. Fetch problem details from Problems API
            // 2. Generate embeddings for title, description, and tags
            // 3. Store the vector in Qdrant with metadata

            _logger.LogInformation("Indexing problem {ProblemId}", problemId);
            await Task.Delay(100, cancellationToken); // Simulate indexing work
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing problem {ProblemId}", problemId);
            throw;
        }
    }

    public async Task UpdateProblemIndexAsync(string problemId, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would:
            // 1. Delete the existing vector
            // 2. Re-index with updated problem data

            _logger.LogInformation("Updating problem index {ProblemId}", problemId);
            await Task.Delay(100, cancellationToken); // Simulate update work
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating problem index {ProblemId}", problemId);
            throw;
        }
    }

    public async Task DeleteProblemIndexAsync(string problemId, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would delete the vector from Qdrant
            _logger.LogInformation("Deleting problem index {ProblemId}", problemId);
            await Task.Delay(50, cancellationToken); // Simulate deletion work
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting problem index {ProblemId}", problemId);
            throw;
        }
    }

    private List<ProblemDto> GetSampleProblems()
    {
        return new List<ProblemDto>
        {
            new()
            {
                Id = "1",
                Title = "Two Sum",
                Slug = "two-sum",
                Description = "Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.",
                Difficulty = "Easy",
                CategoryId = "arrays",
                Tags = new List<string> { "array", "hash-table" },
                IsPaid = false,
                Status = "approved",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30),
                OrderIndex = 1,
                Statistics = new ProblemStatisticsDto
                {
                    TotalSubmissions = 1245,
                    AcceptedSubmissions = 867,
                    AcceptanceRate = 69.6,
                    Views = 5420,
                    Likes = 342,
                    Dislikes = 23
                }
            },
            new()
            {
                Id = "2", 
                Title = "Add Two Numbers",
                Slug = "add-two-numbers",
                Description = "You are given two non-empty linked lists representing two non-negative integers.",
                Difficulty = "Medium",
                CategoryId = "linked-lists",
                Tags = new List<string> { "linked-list", "math", "recursion" },
                IsPaid = false,
                Status = "approved",
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-25),
                OrderIndex = 2,
                Statistics = new ProblemStatisticsDto
                {
                    TotalSubmissions = 987,
                    AcceptedSubmissions = 543,
                    AcceptanceRate = 55.0,
                    Views = 3210,
                    Likes = 287,
                    Dislikes = 45
                }
            },
            new()
            {
                Id = "3",
                Title = "Longest Substring Without Repeating Characters",
                Slug = "longest-substring-without-repeating-characters",
                Description = "Given a string s, find the length of the longest substring without repeating characters.",
                Difficulty = "Medium",
                CategoryId = "strings",
                Tags = new List<string> { "hash-table", "string", "sliding-window" },
                IsPaid = false,
                Status = "approved",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-20),
                OrderIndex = 3,
                Statistics = new ProblemStatisticsDto
                {
                    TotalSubmissions = 2345,
                    AcceptedSubmissions = 1123,
                    AcceptanceRate = 47.9,
                    Views = 8765,
                    Likes = 543,
                    Dislikes = 87
                }
            },
            new()
            {
                Id = "4",
                Title = "Median of Two Sorted Arrays",
                Slug = "median-of-two-sorted-arrays",
                Description = "Given two sorted arrays nums1 and nums2 of size m and n respectively, return the median of the two sorted arrays.",
                Difficulty = "Hard",
                CategoryId = "arrays",
                Tags = new List<string> { "array", "binary-search", "divide-and-conquer" },
                IsPaid = true,
                Status = "approved",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15),
                OrderIndex = 4,
                Statistics = new ProblemStatisticsDto
                {
                    TotalSubmissions = 567,
                    AcceptedSubmissions = 145,
                    AcceptanceRate = 25.6,
                    Views = 2134,
                    Likes = 189,
                    Dislikes = 67
                }
            },
            new()
            {
                Id = "5",
                Title = "Longest Palindromic Substring",
                Slug = "longest-palindromic-substring",
                Description = "Given a string s, return the longest palindromic substring in s.",
                Difficulty = "Medium",
                CategoryId = "strings",
                Tags = new List<string> { "string", "dynamic-programming" },
                IsPaid = false,
                Status = "approved",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10),
                OrderIndex = 5,
                Statistics = new ProblemStatisticsDto
                {
                    TotalSubmissions = 1876,
                    AcceptedSubmissions = 934,
                    AcceptanceRate = 49.8,
                    Views = 6543,
                    Likes = 432,
                    Dislikes = 56
                }
            }
        };
    }
}