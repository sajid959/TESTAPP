using DSAGrind.Models.DTOs;

namespace DSAGrind.Search.API.Services;

public interface IVectorSearchService
{
    Task<List<ProblemDto>> SearchProblemsAsync(string query, int limit, CancellationToken cancellationToken = default);
    Task<List<ProblemDto>> GetSimilarProblemsAsync(string problemId, int count, CancellationToken cancellationToken = default);
    Task<List<ProblemDto>> GetPersonalizedRecommendationsAsync(string userId, int count, CancellationToken cancellationToken = default);
    Task<List<ProblemDto>> SearchByTagsAsync(List<string> tags, int count, CancellationToken cancellationToken = default);
    Task IndexProblemAsync(string problemId, CancellationToken cancellationToken = default);
    Task UpdateProblemIndexAsync(string problemId, CancellationToken cancellationToken = default);
    Task DeleteProblemIndexAsync(string problemId, CancellationToken cancellationToken = default);
}