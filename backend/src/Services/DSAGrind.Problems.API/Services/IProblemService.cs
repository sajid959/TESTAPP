using DSAGrind.Models.DTOs;

namespace DSAGrind.Problems.API.Services;

public interface IProblemService
{
    Task<ProblemDto?> GetProblemAsync(string id, string? userId = null, CancellationToken cancellationToken = default);
    Task<ProblemDto?> GetProblemBySlugAsync(string slug, string? userId = null, CancellationToken cancellationToken = default);
    Task<ProblemSearchResponseDto> SearchProblemsAsync(ProblemSearchRequestDto request, CancellationToken cancellationToken = default);
    Task<List<ProblemDto>> GetRecommendedProblemsAsync(string userId, int count, CancellationToken cancellationToken = default);
    Task<List<ProblemDto>> GetRandomProblemsAsync(int count, string? difficulty = null, CancellationToken cancellationToken = default);
    Task<ProblemDto> CreateProblemAsync(CreateProblemRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<ProblemDto?> UpdateProblemAsync(string id, UpdateProblemRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteProblemAsync(string id, string userId, CancellationToken cancellationToken = default);
    Task<bool> ApproveProblemAsync(string id, string adminUserId, CancellationToken cancellationToken = default);
    Task<List<ProblemDto>> BulkImportProblemsAsync(BulkImportRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<bool> LikeProblemAsync(string problemId, string userId, bool isLike, CancellationToken cancellationToken = default);
    Task<List<ProblemDto>> GetSimilarProblemsAsync(string problemId, int count, CancellationToken cancellationToken = default);
}