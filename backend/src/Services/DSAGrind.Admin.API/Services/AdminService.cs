using DSAGrind.Common.Repositories;
using DSAGrind.Models.Entities;
using DSAGrind.Models.DTOs;
using DSAGrind.Common.Services;
using AutoMapper;
using MongoDB.Bson;

namespace DSAGrind.Admin.API.Services;

public class AdminService : IAdminService
{
    private readonly IMongoRepository<User> _userRepository;
    private readonly IMongoRepository<Problem> _problemRepository;
    private readonly IMongoRepository<Submission> _submissionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IMongoRepository<User> userRepository,
        IMongoRepository<Problem> problemRepository,
        IMongoRepository<Submission> submissionRepository,
        IEventPublisher eventPublisher,
        ILogger<AdminService> logger)
    {
        _userRepository = userRepository;
        _problemRepository = problemRepository;
        _submissionRepository = submissionRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var totalUsers = await _userRepository.CountAsync(cancellationToken: cancellationToken);
            var activeUsers = await _userRepository.CountAsync(u => u.LastLoginAt >= DateTime.UtcNow.AddDays(-30), cancellationToken);
            var totalProblems = await _problemRepository.CountAsync(cancellationToken: cancellationToken);
            var pendingProblems = await _problemRepository.CountAsync(p => p.Status == "pending", cancellationToken);
            var totalSubmissions = await _submissionRepository.CountAsync(cancellationToken: cancellationToken);
            var todaySubmissions = await _submissionRepository.CountAsync(s => s.CreatedAt >= DateTime.UtcNow.Date, cancellationToken);

            return new AdminDashboardDto
            {
                TotalUsers = (int)totalUsers,
                ActiveUsers = (int)activeUsers,
                TotalProblems = (int)totalProblems,
                PendingProblems = (int)pendingProblems,
                TotalSubmissions = (int)totalSubmissions,
                TodaySubmissions = (int)todaySubmissions,
                Revenue = 12450.00m, // Mock data
                PremiumUsers = (int)(totalUsers * 0.15), // Mock 15% premium rate
                SubmissionsChart = GenerateSubmissionsChart(),
                UsersChart = GenerateUsersChart()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin dashboard");
            return new AdminDashboardDto();
        }
    }

    public async Task<List<AdminNotificationDto>> GetNotificationsAsync(CancellationToken cancellationToken = default)
    {
        // Mock notifications - in real implementation, query notifications collection
        return await Task.FromResult(new List<AdminNotificationDto>
        {
            new() { Id = "1", Title = "New Problem Pending Review", Message = "Problem 'Binary Search Tree' needs approval", Type = "info", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new() { Id = "2", Title = "High Error Rate Detected", Message = "Submissions service showing 5% error rate", Type = "warning", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-4) },
            new() { Id = "3", Title = "System Maintenance Complete", Message = "Database optimization completed successfully", Type = "success", IsRead = true, CreatedAt = DateTime.UtcNow.AddHours(-6) }
        });
    }

    public async Task<bool> MarkNotificationReadAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        // In real implementation, update notification in database
        _logger.LogInformation("Marking notification {NotificationId} as read", notificationId);
        return await Task.FromResult(true);
    }

    public async Task<SystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        var services = new List<ServiceHealthDto>
        {
            new() { Name = "Problems API", IsHealthy = true, Status = "Healthy", ResponseTime = 45.2, LastCheck = DateTime.UtcNow },
            new() { Name = "Submissions API", IsHealthy = true, Status = "Healthy", ResponseTime = 67.8, LastCheck = DateTime.UtcNow },
            new() { Name = "AI API", IsHealthy = true, Status = "Healthy", ResponseTime = 123.4, LastCheck = DateTime.UtcNow },
            new() { Name = "Search API", IsHealthy = true, Status = "Healthy", ResponseTime = 89.1, LastCheck = DateTime.UtcNow },
            new() { Name = "Auth API", IsHealthy = true, Status = "Healthy", ResponseTime = 34.6, LastCheck = DateTime.UtcNow }
        };

        return await Task.FromResult(new SystemHealthDto
        {
            IsHealthy = services.All(s => s.IsHealthy),
            Services = services,
            Database = new DatabaseHealthDto { IsHealthy = true, ResponseTime = 12.3, ConnectionCount = 25 },
            ExternalServices = new ExternalServiceHealthDto
            {
                StripeHealthy = true,
                RedisHealthy = true,
                KafkaHealthy = true,
                QdrantHealthy = true
            }
        });
    }

    public async Task<List<AdminAuditLogDto>> GetAuditLogsAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        // Mock audit logs - in real implementation, query audit logs collection
        var logs = new List<AdminAuditLogDto>
        {
            new() { Id = "1", UserId = "admin1", Action = "APPROVE_PROBLEM", Resource = "problem:123", Details = "Approved problem 'Two Sum'", Timestamp = DateTime.UtcNow.AddHours(-1) },
            new() { Id = "2", UserId = "admin1", Action = "BAN_USER", Resource = "user:456", Details = "Banned user for spam", Timestamp = DateTime.UtcNow.AddHours(-3) },
            new() { Id = "3", UserId = "admin2", Action = "CREATE_CATEGORY", Resource = "category:789", Details = "Created new category 'Dynamic Programming'", Timestamp = DateTime.UtcNow.AddHours(-5) }
        };

        return await Task.FromResult(logs.Skip((page - 1) * pageSize).Take(pageSize).ToList());
    }

    public async Task<AdminDashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        return await GetDashboardAsync(cancellationToken);
    }

    public async Task<object> GetAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        // Mock analytics data
        return await Task.FromResult(new
        {
            TotalRevenue = 125000.50m,
            MonthlyGrowth = 12.5,
            UserRetentionRate = 85.3,
            ProblemCompletionRate = 73.2
        });
    }

    public async Task<bool> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        // Mock backup creation
        _logger.LogInformation("Creating system backup...");
        await Task.Delay(1000, cancellationToken); // Simulate backup time
        return true;
    }

    // Delegate to UserManagementService methods
    public async Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 50, string? search = null, CancellationToken cancellationToken = default)
    {
        var userService = new UserManagementService(_userRepository, _eventPublisher, _logger);
        return await userService.GetUsersAsync(page, pageSize, search, cancellationToken);
    }

    public async Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userService = new UserManagementService(_userRepository, _eventPublisher, _logger);
        return await userService.GetUserAsync(userId, cancellationToken);
    }

    public async Task<bool> BanUserAsync(string userId, string reason, string adminUserId, CancellationToken cancellationToken = default)
    {
        var userService = new UserManagementService(_userRepository, _eventPublisher, _logger);
        return await userService.BanUserAsync(userId, reason, adminUserId, cancellationToken);
    }

    public async Task<bool> UnbanUserAsync(string userId, string adminUserId, CancellationToken cancellationToken = default)
    {
        var userService = new UserManagementService(_userRepository, _eventPublisher, _logger);
        return await userService.UnbanUserAsync(userId, adminUserId, cancellationToken);
    }

    public async Task<bool> UpdateUserRoleAsync(string userId, string role, string adminUserId, CancellationToken cancellationToken = default)
    {
        var userService = new UserManagementService(_userRepository, _eventPublisher, _logger);
        return await userService.UpdateUserRoleAsync(userId, role, adminUserId, cancellationToken);
    }

    public async Task<bool> DeleteUserAsync(string userId, string adminUserId, CancellationToken cancellationToken = default)
    {
        var userService = new UserManagementService(_userRepository, _eventPublisher, _logger);
        return await userService.DeleteUserAsync(userId, adminUserId, cancellationToken);
    }

    // Delegate to ContentModerationService methods
    public async Task<List<ProblemDto>> GetPendingProblemsAsync(CancellationToken cancellationToken = default)
    {
        var contentService = new ContentModerationService(_problemRepository, _submissionRepository, _eventPublisher, _logger);
        return await contentService.GetPendingProblemsAsync(cancellationToken);
    }

    public async Task<List<SubmissionDto>> GetFlaggedSubmissionsAsync(CancellationToken cancellationToken = default)
    {
        var contentService = new ContentModerationService(_problemRepository, _submissionRepository, _eventPublisher, _logger);
        return await contentService.GetFlaggedSubmissionsAsync(cancellationToken);
    }

    public async Task<bool> ApproveProblemAsync(string problemId, string adminUserId, CancellationToken cancellationToken = default)
    {
        var contentService = new ContentModerationService(_problemRepository, _submissionRepository, _eventPublisher, _logger);
        return await contentService.ApproveProblemAsync(problemId, adminUserId, cancellationToken);
    }

    public async Task<bool> RejectProblemAsync(string problemId, string reason, string adminUserId, CancellationToken cancellationToken = default)
    {
        var contentService = new ContentModerationService(_problemRepository, _submissionRepository, _eventPublisher, _logger);
        return await contentService.RejectProblemAsync(problemId, reason, adminUserId, cancellationToken);
    }

    public async Task<bool> RemoveSubmissionAsync(string submissionId, string reason, string adminUserId, CancellationToken cancellationToken = default)
    {
        var contentService = new ContentModerationService(_problemRepository, _submissionRepository, _eventPublisher, _logger);
        return await contentService.RemoveSubmissionAsync(submissionId, reason, adminUserId, cancellationToken);
    }

    private List<DashboardChartDto> GenerateSubmissionsChart()
    {
        return new List<DashboardChartDto>
        {
            new() { Label = "Mon", Value = 245 },
            new() { Label = "Tue", Value = 312 },
            new() { Label = "Wed", Value = 189 },
            new() { Label = "Thu", Value = 421 },
            new() { Label = "Fri", Value = 356 },
            new() { Label = "Sat", Value = 278 },
            new() { Label = "Sun", Value = 234 }
        };
    }

    private List<DashboardChartDto> GenerateUsersChart()
    {
        return new List<DashboardChartDto>
        {
            new() { Label = "Jan", Value = 1240 },
            new() { Label = "Feb", Value = 1456 },
            new() { Label = "Mar", Value = 1678 },
            new() { Label = "Apr", Value = 1892 },
            new() { Label = "May", Value = 2134 },
            new() { Label = "Jun", Value = 2387 }
        };
    }
}

public class UserManagementService : IUserManagementService
{
    private readonly IMongoRepository<User> _userRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IMongoRepository<User> userRepository,
        IEventPublisher eventPublisher,
        ILogger<UserManagementService> logger)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 50, string? search = null, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetManyAsync(
            u => string.IsNullOrEmpty(search) || u.Email.Contains(search) || u.FirstName.Contains(search) || u.LastName.Contains(search),
            cancellationToken);

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role,
            IsEmailVerified = u.IsEmailVerified,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt
        }).ToList();
    }

    public async Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsEmailVerified = user.IsEmailVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<bool> BanUserAsync(string userId, string reason, string adminUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _userRepository.UpdateAsync(user.Id, user, cancellationToken);
        await _eventPublisher.PublishAsync("user.banned", new { UserId = userId, Reason = reason, AdminUserId = adminUserId }, cancellationToken);

        return true;
    }

    public async Task<bool> UnbanUserAsync(string userId, string adminUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _userRepository.UpdateAsync(user.Id, user, cancellationToken);
        await _eventPublisher.PublishAsync("user.unbanned", new { UserId = userId, AdminUserId = adminUserId }, cancellationToken);

        return true;
    }

    public async Task<bool> UpdateUserRoleAsync(string userId, string role, string adminUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        var oldRole = user.Role;
        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _userRepository.UpdateAsync(user.Id, user, cancellationToken);
        await _eventPublisher.PublishAsync("user.role_updated", new { UserId = userId, OldRole = oldRole, NewRole = role, AdminUserId = adminUserId }, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteUserAsync(string userId, string adminUserId, CancellationToken cancellationToken = default)
    {
        var success = await _userRepository.DeleteAsync(userId, cancellationToken);
        if (success)
        {
            await _eventPublisher.PublishAsync("user.deleted", new { UserId = userId, AdminUserId = adminUserId }, cancellationToken);
        }

        return success;
    }
}

public class ContentModerationService : IContentModerationService
{
    private readonly IMongoRepository<Problem> _problemRepository;
    private readonly IMongoRepository<Submission> _submissionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ContentModerationService> _logger;

    public ContentModerationService(
        IMongoRepository<Problem> problemRepository,
        IMongoRepository<Submission> submissionRepository,
        IEventPublisher eventPublisher,
        ILogger<ContentModerationService> logger)
    {
        _problemRepository = problemRepository;
        _submissionRepository = submissionRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<List<ProblemDto>> GetPendingProblemsAsync(CancellationToken cancellationToken = default)
    {
        var problems = await _problemRepository.GetManyAsync(p => p.Status == "pending", cancellationToken);
        
        return problems.Select(p => new ProblemDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            Difficulty = p.Difficulty,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            CreatedBy = p.CreatedBy
        }).ToList();
    }

    public async Task<List<SubmissionDto>> GetFlaggedSubmissionsAsync(CancellationToken cancellationToken = default)
    {
        // In real implementation, query submissions flagged by users or AI
        var submissions = await _submissionRepository.GetManyAsync(s => s.Notes != null && s.Notes.Contains("flagged"), cancellationToken);
        
        return submissions.Select(s => new SubmissionDto
        {
            Id = s.Id,
            UserId = s.UserId,
            ProblemId = s.ProblemId,
            Code = s.Code,
            Language = s.Language,
            Status = s.Status,
            CreatedAt = s.CreatedAt,
            Notes = s.Notes
        }).ToList();
    }

    public async Task<bool> ApproveProblemAsync(string problemId, string adminUserId, CancellationToken cancellationToken = default)
    {
        var problem = await _problemRepository.GetByIdAsync(problemId, cancellationToken);
        if (problem == null) return false;

        problem.Status = "approved";
        problem.UpdatedAt = DateTime.UtcNow;
        problem.UpdatedBy = adminUserId;

        await _problemRepository.UpdateAsync(problem.Id, problem, cancellationToken);
        await _eventPublisher.PublishAsync("problem.approved", new { ProblemId = problemId, AdminUserId = adminUserId }, cancellationToken);

        return true;
    }

    public async Task<bool> RejectProblemAsync(string problemId, string reason, string adminUserId, CancellationToken cancellationToken = default)
    {
        var problem = await _problemRepository.GetByIdAsync(problemId, cancellationToken);
        if (problem == null) return false;

        problem.Status = "rejected";
        problem.UpdatedAt = DateTime.UtcNow;
        problem.UpdatedBy = adminUserId;

        await _problemRepository.UpdateAsync(problem.Id, problem, cancellationToken);
        await _eventPublisher.PublishAsync("problem.rejected", new { ProblemId = problemId, Reason = reason, AdminUserId = adminUserId }, cancellationToken);

        return true;
    }

    public async Task<bool> RemoveSubmissionAsync(string submissionId, string reason, string adminUserId, CancellationToken cancellationToken = default)
    {
        var success = await _submissionRepository.DeleteAsync(submissionId, cancellationToken);
        if (success)
        {
            await _eventPublisher.PublishAsync("submission.removed", new { SubmissionId = submissionId, Reason = reason, AdminUserId = adminUserId }, cancellationToken);
        }

        return success;
    }
}