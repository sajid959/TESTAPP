using DSAGrind.Common.Repositories;
using DSAGrind.Models.Entities;
using DSAGrind.Models.DTOs;

namespace DSAGrind.Problems.API.Repositories;

public interface IProblemRepository : IMongoRepository<Problem>
{
    Task<Problem?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Problem>> GetByCategoryAsync(string categoryId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Problem>> SearchProblemsAsync(ProblemSearchRequestDto request, CancellationToken cancellationToken = default);
    Task<List<Problem>> GetRandomProblemsAsync(int count, string? difficulty = null, CancellationToken cancellationToken = default);
    Task<List<Problem>> GetRecommendedProblemsAsync(string userId, int count, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatisticsAsync(string problemId, ProblemStatistics statistics, CancellationToken cancellationToken = default);
    Task<bool> IncrementViewAsync(string problemId, CancellationToken cancellationToken = default);
    Task<bool> UpdateDifficultyRatingAsync(string problemId, string difficulty, CancellationToken cancellationToken = default);
    Task<List<Problem>> GetSimilarProblemsAsync(string problemId, int count, CancellationToken cancellationToken = default);
}