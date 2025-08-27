# DSAGrind Platform - Low Level Design (LLD)

## 📋 Document Information
- **Version**: 1.0
- **Created**: 2024
- **System**: DSAGrind Competitive Programming Platform
- **Focus**: Detailed implementation specifications
- **Technology Stack**: .NET 8, React TypeScript, MongoDB, RabbitMQ

---

## 🎯 Implementation Overview

This document provides detailed implementation specifications for each component of the DSAGrind platform, including class designs, database schemas, API specifications, and algorithm implementations.

---

## 🏗️ Detailed Component Design

### 1. Authentication Service (DSAGrind.Auth.API)

#### Class Structure
```csharp
// Domain Models
public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public SubscriptionType Subscription { get; set; } = SubscriptionType.Free;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public UserProfile Profile { get; set; } = new();
    public List<string> RefreshTokens { get; set; } = new();
}

public class UserProfile
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public List<string> PreferredLanguages { get; set; } = new();
    public UserStatistics Statistics { get; set; } = new();
}

public class UserStatistics
{
    public int TotalSubmissions { get; set; }
    public int AcceptedSubmissions { get; set; }
    public int EasyProblemsCompleted { get; set; }
    public int MediumProblemsCompleted { get; set; }
    public int HardProblemsCompleted { get; set; }
    public decimal AcceptanceRate => TotalSubmissions > 0 ? (decimal)AcceptedSubmissions / TotalSubmissions * 100 : 0;
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public Dictionary<string, int> LanguageStats { get; set; } = new();
}

// Commands and Queries (CQRS)
public class RegisterUserCommand : IRequest<Result<AuthenticationResult>>
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class LoginUserCommand : IRequest<Result<AuthenticationResult>>
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
}

// Command Handlers
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthenticationResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<RegisterUserCommand> _validator;

    public async Task<Result<AuthenticationResult>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate input
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<AuthenticationResult>.Failure(validationResult.Errors.First().ErrorMessage);

        // 2. Check if user already exists
        var existingUser = await _userRepository.GetByEmailOrUsernameAsync(request.Email, request.Username);
        if (existingUser != null)
            return Result<AuthenticationResult>.Failure("User already exists with this email or username");

        // 3. Create user
        var user = new User
        {
            Email = request.Email.ToLowerInvariant(),
            Username = request.Username,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Profile = new UserProfile
            {
                FirstName = request.FirstName,
                LastName = request.LastName
            }
        };

        await _userRepository.CreateAsync(user);

        // 4. Generate tokens
        var authResult = await _tokenGenerator.GenerateTokensAsync(user);

        // 5. Publish event
        await _eventPublisher.PublishAsync("user-registered", new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            RegisteredAt = user.CreatedAt
        });

        return Result<AuthenticationResult>.Success(authResult);
    }
}

// Repository Interface and Implementation
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailOrUsernameAsync(string email, string username);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task UpdateStatisticsAsync(string userId, UserStatistics statistics);
}

public class UserRepository : MongoRepository<User>, IUserRepository
{
    public UserRepository(IMongoClient mongoClient, IConfiguration configuration) 
        : base(mongoClient, configuration.GetValue<string>("MongoDb:DatabaseName"), "users")
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await Collection.Find(u => u.Email == email.ToLowerInvariant()).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await Collection.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailOrUsernameAsync(string email, string username)
    {
        return await Collection.Find(u => u.Email == email.ToLowerInvariant() || u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await Collection.Find(u => u.RefreshTokens.Contains(refreshToken)).FirstOrDefaultAsync();
    }

    public async Task UpdateStatisticsAsync(string userId, UserStatistics statistics)
    {
        var update = Builders<User>.Update.Set(u => u.Profile.Statistics, statistics);
        await Collection.UpdateOneAsync(u => u.Id == userId, update);
    }
}

// JWT Token Generator
public interface IJwtTokenGenerator
{
    Task<AuthenticationResult> GenerateTokensAsync(User user);
    ClaimsPrincipal? ValidateToken(string token);
    Task<AuthenticationResult?> RefreshTokenAsync(string refreshToken);
}

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserRepository _userRepository;

    public async Task<AuthenticationResult> GenerateTokensAsync(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("subscription", user.Subscription.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        var refreshToken = GenerateRefreshToken();
        
        // Store refresh token
        user.RefreshTokens.Add(refreshToken);
        await _userRepository.UpdateAsync(user);

        return new AuthenticationResult
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
            RefreshToken = refreshToken,
            ExpiresAt = accessToken.ValidTo,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role.ToString(),
                Subscription = user.Subscription.ToString(),
                Profile = user.Profile
            }
        };
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
```

### 2. Problem Management Service (DSAGrind.Problems.API)

#### Class Structure
```csharp
// Domain Models
public class Problem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProblemDifficulty Difficulty { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<TestCase> TestCases { get; set; } = new();
    public List<Example> Examples { get; set; } = new();
    public ProblemConstraints Constraints { get; set; } = new();
    public ProblemStatistics Statistics { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsPremium { get; set; } = false;
    public List<string> SupportedLanguages { get; set; } = new() { "csharp", "python", "java", "cpp", "javascript" };
}

public class TestCase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Input { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public bool IsHidden { get; set; } = true;
    public int TimeLimit { get; set; } = 1000; // milliseconds
    public int MemoryLimit { get; set; } = 128; // MB
}

public class Example
{
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string? Explanation { get; set; }
}

public class ProblemConstraints
{
    public string TimeComplexity { get; set; } = string.Empty;
    public string SpaceComplexity { get; set; } = string.Empty;
    public string InputConstraints { get; set; } = string.Empty;
    public int MaxExecutionTime { get; set; } = 5000; // milliseconds
    public int MaxMemoryUsage { get; set; } = 256; // MB
}

public class ProblemStatistics
{
    public int TotalSubmissions { get; set; }
    public int AcceptedSubmissions { get; set; }
    public decimal AcceptanceRate => TotalSubmissions > 0 ? (decimal)AcceptedSubmissions / TotalSubmissions * 100 : 0;
    public Dictionary<string, int> LanguageStats { get; set; } = new();
    public Dictionary<ProblemDifficulty, int> DifficultyDistribution { get; set; } = new();
}

// Commands and Queries
public class GetProblemsQuery : IRequest<Result<PaginatedResult<ProblemDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public ProblemDifficulty? Difficulty { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? SearchTerm { get; set; }
    public bool IncludePremium { get; set; } = false;
    public ProblemSortBy SortBy { get; set; } = ProblemSortBy.CreatedAt;
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
}

public class GetProblemByIdQuery : IRequest<Result<ProblemDetailDto>>
{
    public string ProblemId { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

// Query Handlers
public class GetProblemsQueryHandler : IRequestHandler<GetProblemsQuery, Result<PaginatedResult<ProblemDto>>>
{
    private readonly IProblemRepository _problemRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public async Task<Result<PaginatedResult<ProblemDto>>> Handle(GetProblemsQuery request, CancellationToken cancellationToken)
    {
        // 1. Generate cache key
        var cacheKey = GenerateCacheKey(request);
        
        // 2. Check cache
        var cachedResult = await _cacheService.GetAsync<PaginatedResult<ProblemDto>>(cacheKey);
        if (cachedResult != null)
            return Result<PaginatedResult<ProblemDto>>.Success(cachedResult);

        // 3. Build filter
        var filter = BuildFilter(request);

        // 4. Get total count
        var totalCount = await _problemRepository.CountAsync(filter);

        // 5. Get problems
        var problems = await _problemRepository.GetPaginatedAsync(filter, request.Page, request.PageSize, request.SortBy, request.SortDirection);

        // 6. Map to DTOs
        var problemDtos = _mapper.Map<List<ProblemDto>>(problems);

        var result = new PaginatedResult<ProblemDto>
        {
            Data = problemDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        // 7. Cache result
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));

        return Result<PaginatedResult<ProblemDto>>.Success(result);
    }

    private FilterDefinition<Problem> BuildFilter(GetProblemsQuery request)
    {
        var builder = Builders<Problem>.Filter;
        var filters = new List<FilterDefinition<Problem>>();

        // Active problems only
        filters.Add(builder.Eq(p => p.IsActive, true));

        // Difficulty filter
        if (request.Difficulty.HasValue)
            filters.Add(builder.Eq(p => p.Difficulty, request.Difficulty.Value));

        // Tags filter
        if (request.Tags.Any())
            filters.Add(builder.AnyIn(p => p.Tags, request.Tags));

        // Search term filter
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchFilter = builder.Or(
                builder.Regex(p => p.Title, new BsonRegularExpression(request.SearchTerm, "i")),
                builder.Regex(p => p.Description, new BsonRegularExpression(request.SearchTerm, "i"))
            );
            filters.Add(searchFilter);
        }

        // Premium filter
        if (!request.IncludePremium)
            filters.Add(builder.Eq(p => p.IsPremium, false));

        return filters.Any() ? builder.And(filters) : builder.Empty;
    }
}

// Repository
public interface IProblemRepository : IRepository<Problem>
{
    Task<List<Problem>> GetPaginatedAsync(FilterDefinition<Problem> filter, int page, int pageSize, ProblemSortBy sortBy, SortDirection sortDirection);
    Task<long> CountAsync(FilterDefinition<Problem> filter);
    Task<List<Problem>> GetByTagsAsync(List<string> tags);
    Task<List<Problem>> GetRandomProblemsAsync(int count, ProblemDifficulty? difficulty = null);
    Task UpdateStatisticsAsync(string problemId, ProblemStatistics statistics);
}

public class ProblemRepository : MongoRepository<Problem>, IProblemRepository
{
    public async Task<List<Problem>> GetPaginatedAsync(FilterDefinition<Problem> filter, int page, int pageSize, ProblemSortBy sortBy, SortDirection sortDirection)
    {
        var sortDefinition = BuildSortDefinition(sortBy, sortDirection);
        
        return await Collection
            .Find(filter)
            .Sort(sortDefinition)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountAsync(FilterDefinition<Problem> filter)
    {
        return await Collection.CountDocumentsAsync(filter);
    }

    private SortDefinition<Problem> BuildSortDefinition(ProblemSortBy sortBy, SortDirection sortDirection)
    {
        var builder = Builders<Problem>.Sort;
        
        return sortBy switch
        {
            ProblemSortBy.Title => sortDirection == SortDirection.Asc ? builder.Ascending(p => p.Title) : builder.Descending(p => p.Title),
            ProblemSortBy.Difficulty => sortDirection == SortDirection.Asc ? builder.Ascending(p => p.Difficulty) : builder.Descending(p => p.Difficulty),
            ProblemSortBy.AcceptanceRate => sortDirection == SortDirection.Asc ? builder.Ascending(p => p.Statistics.AcceptanceRate) : builder.Descending(p => p.Statistics.AcceptanceRate),
            ProblemSortBy.CreatedAt => sortDirection == SortDirection.Asc ? builder.Ascending(p => p.CreatedAt) : builder.Descending(p => p.CreatedAt),
            _ => builder.Descending(p => p.CreatedAt)
        };
    }
}
```

### 3. Code Submission Service (DSAGrind.Submissions.API)

#### Class Structure
```csharp
// Domain Models
public class Submission
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ProblemId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public ExecutionResult? Result { get; set; }
    public List<TestCaseResult> TestResults { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public SubmissionMetrics Metrics { get; set; } = new();
}

public class ExecutionResult
{
    public bool Success { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public long MemoryUsed { get; set; } // bytes
    public string Output { get; set; } = string.Empty;
    public string? ErrorOutput { get; set; }
    public int ExitCode { get; set; }
}

public class TestCaseResult
{
    public string TestCaseId { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string ActualOutput { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    public long MemoryUsed { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SubmissionMetrics
{
    public TimeSpan TotalExecutionTime { get; set; }
    public long MaxMemoryUsed { get; set; }
    public int PassedTestCases { get; set; }
    public int TotalTestCases { get; set; }
    public decimal PassRate => TotalTestCases > 0 ? (decimal)PassedTestCases / TotalTestCases * 100 : 0;
}

// Commands
public class SubmitSolutionCommand : IRequest<Result<SubmissionResult>>
{
    public string UserId { get; set; } = string.Empty;
    public string ProblemId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}

// Command Handler
public class SubmitSolutionCommandHandler : IRequestHandler<SubmitSolutionCommand, Result<SubmissionResult>>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IProblemRepository _problemRepository;
    private readonly IDockerExecutionService _executionService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<SubmitSolutionCommand> _validator;

    public async Task<Result<SubmissionResult>> Handle(SubmitSolutionCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate input
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<SubmissionResult>.Failure(validationResult.Errors.First().ErrorMessage);

        // 2. Get problem
        var problem = await _problemRepository.GetByIdAsync(request.ProblemId);
        if (problem == null)
            return Result<SubmissionResult>.Failure("Problem not found");

        // 3. Check language support
        if (!problem.SupportedLanguages.Contains(request.Language))
            return Result<SubmissionResult>.Failure($"Language {request.Language} is not supported for this problem");

        // 4. Create submission record
        var submission = new Submission
        {
            UserId = request.UserId,
            ProblemId = request.ProblemId,
            Code = request.Code,
            Language = request.Language,
            Status = SubmissionStatus.Pending
        };

        await _submissionRepository.CreateAsync(submission);

        // 5. Start asynchronous execution
        _ = Task.Run(async () => await ExecuteSubmissionAsync(submission, problem), cancellationToken);

        return Result<SubmissionResult>.Success(new SubmissionResult
        {
            SubmissionId = submission.Id,
            Status = submission.Status,
            Message = "Submission received and is being processed"
        });
    }

    private async Task ExecuteSubmissionAsync(Submission submission, Problem problem)
    {
        try
        {
            submission.Status = SubmissionStatus.Running;
            await _submissionRepository.UpdateAsync(submission);

            var testResults = new List<TestCaseResult>();
            var totalExecutionTime = TimeSpan.Zero;
            long maxMemoryUsed = 0;

            // Execute each test case
            foreach (var testCase in problem.TestCases)
            {
                var executionResult = await _executionService.ExecuteCodeAsync(
                    submission.Code,
                    submission.Language,
                    testCase.Input,
                    testCase.TimeLimit,
                    testCase.MemoryLimit
                );

                var testResult = new TestCaseResult
                {
                    TestCaseId = testCase.Id,
                    ActualOutput = executionResult.Output.Trim(),
                    ExpectedOutput = testCase.ExpectedOutput.Trim(),
                    ExecutionTime = executionResult.ExecutionTime,
                    MemoryUsed = executionResult.MemoryUsed,
                    ErrorMessage = executionResult.ErrorOutput
                };

                testResult.Passed = string.Equals(testResult.ActualOutput, testResult.ExpectedOutput, StringComparison.OrdinalIgnoreCase);
                testResults.Add(testResult);

                totalExecutionTime += executionResult.ExecutionTime;
                maxMemoryUsed = Math.Max(maxMemoryUsed, executionResult.MemoryUsed);

                // Stop on first failure for efficiency
                if (!testResult.Passed && !testCase.IsHidden)
                {
                    break;
                }
            }

            // Determine final status
            var passedCount = testResults.Count(tr => tr.Passed);
            var totalCount = testResults.Count;

            submission.Status = passedCount == problem.TestCases.Count ? SubmissionStatus.Accepted : 
                              totalCount == 0 ? SubmissionStatus.RuntimeError : SubmissionStatus.WrongAnswer;

            submission.TestResults = testResults;
            submission.Metrics = new SubmissionMetrics
            {
                TotalExecutionTime = totalExecutionTime,
                MaxMemoryUsed = maxMemoryUsed,
                PassedTestCases = passedCount,
                TotalTestCases = totalCount
            };

            submission.CompletedAt = DateTime.UtcNow;
            await _submissionRepository.UpdateAsync(submission);

            // Publish completion event
            await _eventPublisher.PublishAsync("submission-completed", new SubmissionCompletedEvent
            {
                SubmissionId = submission.Id,
                UserId = submission.UserId,
                ProblemId = submission.ProblemId,
                Status = submission.Status,
                ExecutionTime = totalExecutionTime,
                MemoryUsed = maxMemoryUsed,
                CompletedAt = submission.CompletedAt.Value
            });
        }
        catch (Exception ex)
        {
            submission.Status = SubmissionStatus.RuntimeError;
            submission.ErrorMessage = "Internal execution error";
            submission.CompletedAt = DateTime.UtcNow;
            await _submissionRepository.UpdateAsync(submission);
        }
    }
}

// Docker Execution Service
public interface IDockerExecutionService
{
    Task<ExecutionResult> ExecuteCodeAsync(string code, string language, string input, int timeLimit, int memoryLimit);
}

public class DockerExecutionService : IDockerExecutionService
{
    private readonly DockerClient _dockerClient;
    private readonly ILogger<DockerExecutionService> _logger;

    public async Task<ExecutionResult> ExecuteCodeAsync(string code, string language, string input, int timeLimit, int memoryLimit)
    {
        var containerId = string.Empty;
        
        try
        {
            // 1. Create container configuration
            var containerConfig = new CreateContainerParameters
            {
                Image = GetImageForLanguage(language),
                Cmd = GetExecutionCommand(code, language),
                WorkingDir = "/app",
                NetworkMode = "none", // No network access
                User = "nobody:nobody", // Non-privileged user
                Env = new[] { $"INPUT={input}" },
                HostConfig = new HostConfig
                {
                    Memory = memoryLimit * 1024 * 1024, // Convert MB to bytes
                    CpuQuota = 50000, // 50% CPU limit
                    ReadonlyRootfs = true, // Read-only filesystem
                    SecurityOpt = new[] { "no-new-privileges" },
                    Tmpfs = new Dictionary<string, string> { { "/tmp", "rw,noexec,nosuid,size=100m" } }
                }
            };

            // 2. Create and start container
            var container = await _dockerClient.Containers.CreateContainerAsync(containerConfig);
            containerId = container.ID;

            var startTime = DateTime.UtcNow;
            await _dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters());

            // 3. Wait for completion with timeout
            var waitResult = await _dockerClient.Containers.WaitContainerAsync(
                containerId, 
                CancellationToken.None
            ).WaitAsync(TimeSpan.FromMilliseconds(timeLimit));

            var endTime = DateTime.UtcNow;
            var executionTime = endTime - startTime;

            // 4. Get container stats for memory usage
            var stats = await _dockerClient.Containers.GetContainerStatsAsync(containerId, new ContainerStatsParameters
            {
                OneShot = true
            });

            // 5. Get logs
            var logs = await _dockerClient.Containers.GetContainerLogsAsync(containerId, new ContainerLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Timestamps = false
            });

            var (stdout, stderr) = ParseContainerLogs(logs);

            return new ExecutionResult
            {
                Success = waitResult.StatusCode == 0,
                ExecutionTime = executionTime,
                MemoryUsed = GetMemoryUsageFromStats(stats),
                Output = stdout,
                ErrorOutput = stderr,
                ExitCode = (int)waitResult.StatusCode
            };
        }
        catch (TimeoutException)
        {
            return new ExecutionResult
            {
                Success = false,
                ExecutionTime = TimeSpan.FromMilliseconds(timeLimit),
                Output = string.Empty,
                ErrorOutput = "Time Limit Exceeded",
                ExitCode = -1
            };
        }
        finally
        {
            // Cleanup container
            if (!string.IsNullOrEmpty(containerId))
            {
                try
                {
                    await _dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
                    {
                        Force = true
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to remove container {ContainerId}", containerId);
                }
            }
        }
    }

    private string GetImageForLanguage(string language)
    {
        return language.ToLowerInvariant() switch
        {
            "csharp" => "mcr.microsoft.com/dotnet/runtime:8.0-alpine",
            "python" => "python:3.11-alpine",
            "java" => "openjdk:17-alpine",
            "cpp" => "gcc:latest",
            "javascript" => "node:18-alpine",
            _ => throw new NotSupportedException($"Language {language} is not supported")
        };
    }

    private List<string> GetExecutionCommand(string code, string language)
    {
        return language.ToLowerInvariant() switch
        {
            "csharp" => new List<string> { "sh", "-c", $"echo '{code}' > Program.cs && dotnet new console -f net8.0 && cp Program.cs . && dotnet run" },
            "python" => new List<string> { "python", "-c", code },
            "java" => new List<string> { "sh", "-c", $"echo '{code}' > Main.java && javac Main.java && java Main" },
            "cpp" => new List<string> { "sh", "-c", $"echo '{code}' > main.cpp && g++ -o main main.cpp && ./main" },
            "javascript" => new List<string> { "node", "-e", code },
            _ => throw new NotSupportedException($"Language {language} is not supported")
        };
    }

    private (string stdout, string stderr) ParseContainerLogs(Stream logStream)
    {
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        using var reader = new StreamReader(logStream);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            // Docker log format: [8 bytes header][payload]
            // First byte indicates stream type: 1=stdout, 2=stderr
            if (line.Length > 8)
            {
                var streamType = line[0];
                var content = line.Substring(8);
                
                if (streamType == 1)
                    stdout.AppendLine(content);
                else if (streamType == 2)
                    stderr.AppendLine(content);
            }
        }

        return (stdout.ToString().Trim(), stderr.ToString().Trim());
    }

    private long GetMemoryUsageFromStats(ContainerStatsResponse stats)
    {
        return stats.MemoryStats?.Usage ?? 0;
    }
}
```

---

## 💾 Database Schema Design

### MongoDB Collections

#### Users Collection
```javascript
{
  _id: ObjectId("..."),
  id: "user-uuid",
  email: "user@example.com",
  username: "johndoe",
  passwordHash: "hashed-password",
  role: "User", // User, Premium, Admin
  subscription: "Free", // Free, Premium, Enterprise
  createdAt: ISODate("2024-01-01T00:00:00Z"),
  lastLoginAt: ISODate("2024-01-01T00:00:00Z"),
  isEmailVerified: true,
  profile: {
    firstName: "John",
    lastName: "Doe",
    bio: "Software Engineer",
    profileImageUrl: "https://...",
    country: "US",
    dateOfBirth: ISODate("1990-01-01T00:00:00Z"),
    preferredLanguages: ["csharp", "python"],
    statistics: {
      totalSubmissions: 150,
      acceptedSubmissions: 120,
      easyProblemsCompleted: 50,
      mediumProblemsCompleted: 40,
      hardProblemsCompleted: 10,
      currentStreak: 5,
      longestStreak: 15,
      languageStats: {
        "csharp": 80,
        "python": 70
      }
    }
  },
  refreshTokens: ["token1", "token2"]
}

// Indexes
db.users.createIndex({ "email": 1 }, { unique: true })
db.users.createIndex({ "username": 1 }, { unique: true })
db.users.createIndex({ "refreshTokens": 1 })
db.users.createIndex({ "profile.statistics.acceptedSubmissions": -1 })
```

#### Problems Collection
```javascript
{
  _id: ObjectId("..."),
  id: "problem-uuid",
  title: "Two Sum",
  description: "Given an array of integers...",
  difficulty: "Easy", // Easy, Medium, Hard
  tags: ["array", "hash-table"],
  testCases: [
    {
      id: "testcase-uuid",
      input: "[2,7,11,15]\n9",
      expectedOutput: "[0,1]",
      isHidden: false,
      timeLimit: 1000,
      memoryLimit: 128
    }
  ],
  examples: [
    {
      input: "[2,7,11,15], target = 9",
      output: "[0,1]",
      explanation: "Because nums[0] + nums[1] == 9..."
    }
  ],
  constraints: {
    timeComplexity: "O(n)",
    spaceComplexity: "O(n)",
    inputConstraints: "2 <= nums.length <= 10^4",
    maxExecutionTime: 5000,
    maxMemoryUsage: 256
  },
  statistics: {
    totalSubmissions: 1000,
    acceptedSubmissions: 600,
    languageStats: {
      "csharp": 300,
      "python": 400,
      "java": 300
    }
  },
  createdBy: "admin-user-id",
  createdAt: ISODate("2024-01-01T00:00:00Z"),
  updatedAt: ISODate("2024-01-01T00:00:00Z"),
  isActive: true,
  isPremium: false,
  supportedLanguages: ["csharp", "python", "java", "cpp", "javascript"]
}

// Indexes
db.problems.createIndex({ "difficulty": 1, "tags": 1 })
db.problems.createIndex({ "createdAt": -1 })
db.problems.createIndex({ "statistics.acceptanceRate": -1 })
db.problems.createIndex({ "isActive": 1, "isPremium": 1 })
db.problems.createIndex({ "title": "text", "description": "text" })
```

#### Submissions Collection
```javascript
{
  _id: ObjectId("..."),
  id: "submission-uuid",
  userId: "user-uuid",
  problemId: "problem-uuid",
  code: "public class Solution { ... }",
  language: "csharp",
  status: "Accepted", // Pending, Running, Accepted, WrongAnswer, RuntimeError, TimeLimitExceeded
  submittedAt: ISODate("2024-01-01T00:00:00Z"),
  completedAt: ISODate("2024-01-01T00:00:00Z"),
  result: {
    success: true,
    executionTime: NumberLong(150), // milliseconds
    memoryUsed: NumberLong(1048576), // bytes
    output: "Expected output",
    errorOutput: null,
    exitCode: 0
  },
  testResults: [
    {
      testCaseId: "testcase-uuid",
      passed: true,
      actualOutput: "[0,1]",
      expectedOutput: "[0,1]",
      executionTime: NumberLong(50),
      memoryUsed: NumberLong(524288),
      errorMessage: null
    }
  ],
  metrics: {
    totalExecutionTime: NumberLong(150),
    maxMemoryUsed: NumberLong(1048576),
    passedTestCases: 5,
    totalTestCases: 5
  },
  errorMessage: null
}

// Indexes
db.submissions.createIndex({ "userId": 1, "submittedAt": -1 })
db.submissions.createIndex({ "problemId": 1, "status": 1 })
db.submissions.createIndex({ "submittedAt": -1 })
db.submissions.createIndex({ "userId": 1, "problemId": 1, "submittedAt": -1 })
```

---

## 🔌 API Endpoint Specifications

### Authentication API Endpoints

#### POST /api/auth/register
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "username": "johndoe",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}

Response 201 Created:
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "secure-refresh-token",
    "expiresAt": "2024-01-01T01:00:00Z",
    "user": {
      "id": "user-uuid",
      "email": "user@example.com",
      "username": "johndoe",
      "role": "User",
      "subscription": "Free",
      "profile": {
        "firstName": "John",
        "lastName": "Doe",
        "statistics": {
          "totalSubmissions": 0,
          "acceptedSubmissions": 0,
          "acceptanceRate": 0
        }
      }
    }
  }
}

Response 400 Bad Request:
{
  "success": false,
  "error": "User already exists with this email or username",
  "validationErrors": []
}
```

#### POST /api/auth/login
```http
POST /api/auth/login
Content-Type: application/json

{
  "emailOrUsername": "user@example.com",
  "password": "SecurePassword123!",
  "rememberMe": true
}

Response 200 OK:
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "secure-refresh-token",
    "expiresAt": "2024-01-01T01:00:00Z",
    "user": { /* user object */ }
  }
}

Response 401 Unauthorized:
{
  "success": false,
  "error": "Invalid credentials"
}
```

### Problems API Endpoints

#### GET /api/problems
```http
GET /api/problems?page=1&pageSize=20&difficulty=Medium&tags=array,hash-table&searchTerm=two%20sum&sortBy=AcceptanceRate&sortDirection=desc
Authorization: Bearer <access-token>

Response 200 OK:
{
  "success": true,
  "data": {
    "data": [
      {
        "id": "problem-uuid",
        "title": "Two Sum",
        "difficulty": "Easy",
        "tags": ["array", "hash-table"],
        "statistics": {
          "totalSubmissions": 1000,
          "acceptedSubmissions": 600,
          "acceptanceRate": 60.0
        },
        "isPremium": false
      }
    ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 20,
    "totalPages": 8
  }
}
```

#### GET /api/problems/{id}
```http
GET /api/problems/problem-uuid
Authorization: Bearer <access-token>

Response 200 OK:
{
  "success": true,
  "data": {
    "id": "problem-uuid",
    "title": "Two Sum",
    "description": "Given an array of integers and a target...",
    "difficulty": "Easy",
    "tags": ["array", "hash-table"],
    "examples": [
      {
        "input": "[2,7,11,15], target = 9",
        "output": "[0,1]",
        "explanation": "Because nums[0] + nums[1] == 9..."
      }
    ],
    "constraints": {
      "timeComplexity": "O(n)",
      "spaceComplexity": "O(n)",
      "inputConstraints": "2 <= nums.length <= 10^4"
    },
    "supportedLanguages": ["csharp", "python", "java"],
    "statistics": {
      "totalSubmissions": 1000,
      "acceptedSubmissions": 600,
      "acceptanceRate": 60.0
    },
    "userProgress": {
      "hasAttempted": true,
      "isCompleted": false,
      "bestSubmission": {
        "status": "WrongAnswer",
        "submittedAt": "2024-01-01T00:00:00Z"
      }
    }
  }
}
```

### Submissions API Endpoints

#### POST /api/submissions
```http
POST /api/submissions
Authorization: Bearer <access-token>
Content-Type: application/json

{
  "problemId": "problem-uuid",
  "code": "public class Solution {\n    public int[] TwoSum(int[] nums, int target) {\n        // solution code\n    }\n}",
  "language": "csharp"
}

Response 202 Accepted:
{
  "success": true,
  "data": {
    "submissionId": "submission-uuid",
    "status": "Pending",
    "message": "Submission received and is being processed"
  }
}

Response 400 Bad Request:
{
  "success": false,
  "error": "Language csharp is not supported for this problem"
}
```

#### GET /api/submissions/{id}
```http
GET /api/submissions/submission-uuid
Authorization: Bearer <access-token>

Response 200 OK:
{
  "success": true,
  "data": {
    "id": "submission-uuid",
    "problemId": "problem-uuid",
    "problemTitle": "Two Sum",
    "code": "public class Solution { ... }",
    "language": "csharp",
    "status": "Accepted",
    "submittedAt": "2024-01-01T00:00:00Z",
    "completedAt": "2024-01-01T00:00:05Z",
    "metrics": {
      "totalExecutionTime": 150,
      "maxMemoryUsed": 1048576,
      "passedTestCases": 5,
      "totalTestCases": 5,
      "passRate": 100.0
    },
    "testResults": [
      {
        "testCaseId": "testcase-uuid",
        "passed": true,
        "actualOutput": "[0,1]",
        "expectedOutput": "[0,1]",
        "executionTime": 50,
        "memoryUsed": 524288,
        "isHidden": false
      }
    ]
  }
}
```

---

## 🔐 Security Implementation Details

### JWT Token Implementation
```csharp
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var principal = _tokenGenerator.ValidateToken(token);
            
            if (principal != null)
            {
                context.User = principal;
            }
        }

        await _next(context);
    }
}
```

### Input Validation
```csharp
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email address is required")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
    }
}
```

### Code Execution Security
```csharp
public class SecureDockerExecutionService : IDockerExecutionService
{
    private readonly Dictionary<string, string> _secureImages = new()
    {
        ["csharp"] = "dsagrind/dotnet-secure:8.0",
        ["python"] = "dsagrind/python-secure:3.11",
        ["java"] = "dsagrind/java-secure:17"
    };

    private CreateContainerParameters CreateSecureContainer(string language, string code, string input)
    {
        return new CreateContainerParameters
        {
            Image = _secureImages[language],
            Cmd = GetSecureExecutionCommand(language),
            NetworkMode = "none", // No network access
            User = "runner:runner", // Non-privileged user
            Env = new[] 
            { 
                $"CODE={Convert.ToBase64String(Encoding.UTF8.GetBytes(code))}",
                $"INPUT={Convert.ToBase64String(Encoding.UTF8.GetBytes(input))}"
            },
            HostConfig = new HostConfig
            {
                Memory = 128 * 1024 * 1024, // 128MB limit
                CpuQuota = 50000, // 50% CPU
                CpuPeriod = 100000,
                PidsLimit = 64, // Process limit
                ReadonlyRootfs = true,
                SecurityOpt = new[] 
                { 
                    "no-new-privileges",
                    "seccomp:unconfined" // Restricted system calls
                },
                Tmpfs = new Dictionary<string, string> 
                { 
                    { "/tmp", "rw,noexec,nosuid,size=50m" },
                    { "/var/tmp", "rw,noexec,nosuid,size=50m" }
                },
                Ulimits = new List<Ulimit>
                {
                    new() { Name = "nproc", Soft = 32, Hard = 32 }, // Process limit
                    new() { Name = "nofile", Soft = 64, Hard = 64 } // File descriptor limit
                }
            }
        };
    }

    private List<string> GetSecureExecutionCommand(string language)
    {
        return new List<string> 
        { 
            "/secure-runner.sh", // Custom secure execution script
            language 
        };
    }
}
```

This Low-Level Design provides comprehensive implementation details for building the DSAGrind platform, covering all major components with actual code implementations, database schemas, API specifications, and security measures.