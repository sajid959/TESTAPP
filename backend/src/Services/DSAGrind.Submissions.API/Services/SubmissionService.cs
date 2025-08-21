using DSAGrind.Common.Repositories;
using DSAGrind.Common.Services;
using DSAGrind.Models.Entities;
using AutoMapper;

namespace DSAGrind.Submissions.API.Services;

public class SubmissionService : ISubmissionService
{
    private readonly IMongoRepository<Submission> _submissionRepository;
    private readonly ICodeExecutionService _codeExecutionService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<SubmissionService> _logger;

    public SubmissionService(
        IMongoRepository<Submission> submissionRepository,
        ICodeExecutionService codeExecutionService,
        IEventPublisher eventPublisher,
        IMapper mapper,
        ILogger<SubmissionService> logger)
    {
        _submissionRepository = submissionRepository;
        _codeExecutionService = codeExecutionService;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SubmissionDto?> GetSubmissionAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetByIdAsync(id, cancellationToken);
        if (submission == null || submission.UserId != userId) return null;

        return _mapper.Map<SubmissionDto>(submission);
    }

    public async Task<List<SubmissionDto>> GetUserSubmissionsAsync(string userId, string? problemId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var submissions = await _submissionRepository.GetManyAsync(
            s => s.UserId == userId && (problemId == null || s.ProblemId == problemId),
            page: page,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        return _mapper.Map<List<SubmissionDto>>(submissions);
    }

    public async Task<SubmissionDto> CreateSubmissionAsync(CreateSubmissionRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        var submission = new Submission
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ProblemId = request.ProblemId,
            Code = request.Code,
            Language = request.Language,
            Status = "pending",
            IsPublic = request.IsPublic,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            ExecutionDetails = new ExecutionDetails()
        };

        await _submissionRepository.CreateAsync(submission, cancellationToken);

        // Execute the code
        var executionResult = await _codeExecutionService.ExecuteCodeAsync(request.ProblemId, request.Code, request.Language, cancellationToken);
        
        // Update submission with results
        submission.Status = executionResult.Status;
        submission.Runtime = executionResult.Runtime;
        submission.Memory = executionResult.Memory;
        submission.TestResults = _mapper.Map<List<TestResult>>(executionResult.TestResults);
        submission.ErrorMessage = executionResult.ErrorMessage;
        submission.ExecutionDetails = _mapper.Map<ExecutionDetails>(executionResult.ExecutionDetails);

        await _submissionRepository.UpdateAsync(submission, cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync("submission.created", new { SubmissionId = submission.Id, UserId = userId, Status = submission.Status }, cancellationToken);

        return _mapper.Map<SubmissionDto>(submission);
    }

    public async Task<CodeExecutionResultDto> ExecuteCodeAsync(CodeExecutionRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        return await _codeExecutionService.ExecuteCodeAsync(request.ProblemId, request.Code, request.Language, cancellationToken);
    }

    public async Task<CodeExecutionResultDto> TestCodeAsync(CodeTestRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        return await _codeExecutionService.TestCodeAsync(request.Code, request.Language, request.Input, cancellationToken);
    }

    public async Task<List<string>> GetSupportedLanguagesAsync(CancellationToken cancellationToken = default)
    {
        return await _codeExecutionService.GetSupportedLanguagesAsync(cancellationToken);
    }

    public async Task<List<SubmissionDto>> GetLeaderboardAsync(string problemId, int limit = 10, CancellationToken cancellationToken = default)
    {
        var submissions = await _submissionRepository.GetManyAsync(
            s => s.ProblemId == problemId && s.Status == "accepted",
            limit,
            cancellationToken: cancellationToken);

        // Sort by runtime ascending
        var sortedSubmissions = submissions.OrderBy(s => s.Runtime).Take(limit).ToList();

        return _mapper.Map<List<SubmissionDto>>(sortedSubmissions);
    }
}

public interface ICodeExecutionService
{
    Task<CodeExecutionResultDto> ExecuteCodeAsync(string problemId, string code, string language, CancellationToken cancellationToken = default);
    Task<CodeExecutionResultDto> TestCodeAsync(string code, string language, string input, CancellationToken cancellationToken = default);
    Task<List<string>> GetSupportedLanguagesAsync(CancellationToken cancellationToken = default);
}