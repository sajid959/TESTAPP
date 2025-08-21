using DSAGrind.Problems.API.Repositories;
using DSAGrind.Models.DTOs;
using DSAGrind.Models.Entities;
using DSAGrind.Common.Services;
using AutoMapper;

namespace DSAGrind.Problems.API.Services;

public class ProblemService : IProblemService
{
    private readonly IProblemRepository _problemRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<ProblemService> _logger;

    public ProblemService(
        IProblemRepository problemRepository,
        ICategoryRepository categoryRepository,
        IEventPublisher eventPublisher,
        IMapper mapper,
        ILogger<ProblemService> logger)
    {
        _problemRepository = problemRepository;
        _categoryRepository = categoryRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProblemDto?> GetProblemAsync(string id, string? userId = null, CancellationToken cancellationToken = default)
    {
        var problem = await _problemRepository.GetByIdAsync(id, cancellationToken);
        if (problem == null) return null;

        // Increment view count
        await _problemRepository.IncrementViewAsync(id, cancellationToken);

        return _mapper.Map<ProblemDto>(problem);
    }

    public async Task<ProblemDto?> GetProblemBySlugAsync(string slug, string? userId = null, CancellationToken cancellationToken = default)
    {
        var problem = await _problemRepository.GetBySlugAsync(slug, cancellationToken);
        if (problem == null) return null;

        // Increment view count
        await _problemRepository.IncrementViewAsync(problem.Id, cancellationToken);

        return _mapper.Map<ProblemDto>(problem);
    }

    public async Task<ProblemSearchResponseDto> SearchProblemsAsync(ProblemSearchRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = await _problemRepository.SearchProblemsAsync(request, cancellationToken);
        
        return new ProblemSearchResponseDto
        {
            Problems = _mapper.Map<List<ProblemDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            HasNextPage = result.Page < result.TotalPages,
            HasPreviousPage = result.Page > 1
        };
    }

    public async Task<List<ProblemDto>> GetRecommendedProblemsAsync(string userId, int count, CancellationToken cancellationToken = default)
    {
        var problems = await _problemRepository.GetRecommendedProblemsAsync(userId, count, cancellationToken);
        return _mapper.Map<List<ProblemDto>>(problems);
    }

    public async Task<List<ProblemDto>> GetRandomProblemsAsync(int count, string? difficulty = null, CancellationToken cancellationToken = default)
    {
        var problems = await _problemRepository.GetRandomProblemsAsync(count, difficulty, cancellationToken);
        return _mapper.Map<List<ProblemDto>>(problems);
    }

    public async Task<ProblemDto> CreateProblemAsync(CreateProblemRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        var problem = new Problem
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Description = request.Description,
            Difficulty = request.Difficulty,
            CategoryId = request.CategoryId,
            Tags = request.Tags ?? new List<string>(),
            TestCases = request.TestCases ?? new List<TestCase>(),
            Constraints = request.Constraints ?? new List<string>(),
            Examples = request.Examples ?? new List<ProblemExample>(),
            Hints = request.Hints ?? new List<string>(),
            SolutionTemplate = request.SolutionTemplate ?? new Dictionary<string, string>(),
            IsPaid = request.IsPaid,
            Status = "pending", // Needs approval
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Statistics = new ProblemStatistics()
        };

        await _problemRepository.CreateAsync(problem, cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync("problem.created", new { ProblemId = problem.Id, UserId = userId }, cancellationToken);

        return _mapper.Map<ProblemDto>(problem);
    }

    public async Task<ProblemDto?> UpdateProblemAsync(string id, UpdateProblemRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        var problem = await _problemRepository.GetByIdAsync(id, cancellationToken);
        if (problem == null) return null;

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Title))
        {
            problem.Title = request.Title;
            problem.Slug = GenerateSlug(request.Title);
        }
        
        if (!string.IsNullOrEmpty(request.Description))
            problem.Description = request.Description;

        if (!string.IsNullOrEmpty(request.Difficulty))
            problem.Difficulty = request.Difficulty;

        if (request.Tags != null)
            problem.Tags = request.Tags;

        if (request.TestCases != null)
            problem.TestCases = request.TestCases;

        if (request.Constraints != null)
            problem.Constraints = request.Constraints;

        if (request.Examples != null)
            problem.Examples = request.Examples;

        if (request.Hints != null)
            problem.Hints = request.Hints;

        if (request.IsPaid.HasValue)
            problem.IsPaid = request.IsPaid.Value;

        problem.UpdatedAt = DateTime.UtcNow;
        problem.UpdatedBy = userId;

        await _problemRepository.UpdateAsync(problem, cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync("problem.updated", new { ProblemId = problem.Id, UserId = userId }, cancellationToken);

        return _mapper.Map<ProblemDto>(problem);
    }

    public async Task<bool> DeleteProblemAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var success = await _problemRepository.DeleteAsync(id, cancellationToken);
        
        if (success)
        {
            await _eventPublisher.PublishAsync("problem.deleted", new { ProblemId = id, UserId = userId }, cancellationToken);
        }

        return success;
    }

    public async Task<bool> ApproveProblemAsync(string id, string adminUserId, CancellationToken cancellationToken = default)
    {
        var problem = await _problemRepository.GetByIdAsync(id, cancellationToken);
        if (problem == null) return false;

        problem.Status = "approved";
        problem.UpdatedAt = DateTime.UtcNow;
        problem.UpdatedBy = adminUserId;

        await _problemRepository.UpdateAsync(problem, cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync("problem.approved", new { ProblemId = id, AdminUserId = adminUserId }, cancellationToken);

        return true;
    }

    public async Task<List<ProblemDto>> BulkImportProblemsAsync(BulkImportRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        var problems = new List<Problem>();

        foreach (var item in request.Problems)
        {
            var problem = new Problem
            {
                Id = Guid.NewGuid().ToString(),
                Title = item.Title,
                Slug = GenerateSlug(item.Title),
                Description = item.Description,
                Difficulty = item.Difficulty,
                CategoryId = item.CategoryId,
                Tags = item.Tags ?? new List<string>(),
                IsPaid = item.IsPaid,
                Status = "pending",
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Statistics = new ProblemStatistics()
            };

            problems.Add(problem);
        }

        await _problemRepository.CreateManyAsync(problems, cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync("problems.bulk_imported", new { Count = problems.Count, UserId = userId }, cancellationToken);

        return _mapper.Map<List<ProblemDto>>(problems);
    }

    public async Task<bool> LikeProblemAsync(string problemId, string userId, bool isLike, CancellationToken cancellationToken = default)
    {
        var problem = await _problemRepository.GetByIdAsync(problemId, cancellationToken);
        if (problem == null) return false;

        // In a real implementation, we'd track individual user likes/dislikes
        // For now, just increment/decrement the counters
        if (isLike)
        {
            problem.Statistics.Likes++;
        }
        else
        {
            problem.Statistics.Dislikes++;
        }

        await _problemRepository.UpdateStatisticsAsync(problemId, problem.Statistics, cancellationToken);

        return true;
    }

    public async Task<List<ProblemDto>> GetSimilarProblemsAsync(string problemId, int count, CancellationToken cancellationToken = default)
    {
        var problems = await _problemRepository.GetSimilarProblemsAsync(problemId, count, cancellationToken);
        return _mapper.Map<List<ProblemDto>>(problems);
    }

    private static string GenerateSlug(string title)
    {
        return title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("(", "")
            .Replace(")", "");
    }
}