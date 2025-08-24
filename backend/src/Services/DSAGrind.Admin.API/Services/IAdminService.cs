using DSAGrind.Models.DTOs;

namespace DSAGrind.Admin.API.Services;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<AdminDashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default);
    Task<List<AdminNotificationDto>> GetNotificationsAsync(CancellationToken cancellationToken = default);
    Task<bool> MarkNotificationReadAsync(string notificationId, CancellationToken cancellationToken = default);
    Task<SystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken = default);
    Task<List<AdminAuditLogDto>> GetAuditLogsAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    // User Management Methods
    Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 50, string? search = null, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> BanUserAsync(string userId, string reason, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> UnbanUserAsync(string userId, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserRoleAsync(string userId, string role, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(string userId, string adminUserId, CancellationToken cancellationToken = default);
    
    // Content Moderation Methods
    Task<List<ProblemDto>> GetPendingProblemsAsync(CancellationToken cancellationToken = default);
    Task<List<SubmissionDto>> GetFlaggedSubmissionsAsync(CancellationToken cancellationToken = default);
    Task<bool> ApproveProblemAsync(string problemId, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> RejectProblemAsync(string problemId, string reason, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> RemoveSubmissionAsync(string submissionId, string reason, string adminUserId, CancellationToken cancellationToken = default);
    
    // Analytics and Backup Methods
    Task<object> GetAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<bool> CreateBackupAsync(CancellationToken cancellationToken = default);
}

public interface IUserManagementService
{
    Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 50, string? search = null, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> BanUserAsync(string userId, string reason, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> UnbanUserAsync(string userId, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserRoleAsync(string userId, string role, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(string userId, string adminUserId, CancellationToken cancellationToken = default);
}

public interface IContentModerationService
{
    Task<List<ProblemDto>> GetPendingProblemsAsync(CancellationToken cancellationToken = default);
    Task<List<SubmissionDto>> GetFlaggedSubmissionsAsync(CancellationToken cancellationToken = default);
    Task<bool> ApproveProblemAsync(string problemId, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> RejectProblemAsync(string problemId, string reason, string adminUserId, CancellationToken cancellationToken = default);
    Task<bool> RemoveSubmissionAsync(string submissionId, string reason, string adminUserId, CancellationToken cancellationToken = default);
}

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalProblems { get; set; }
    public int PendingProblems { get; set; }
    public int TotalSubmissions { get; set; }
    public int TodaySubmissions { get; set; }
    public decimal Revenue { get; set; }
    public int PremiumUsers { get; set; }
    public List<DashboardChartDto> SubmissionsChart { get; set; } = new();
    public List<DashboardChartDto> UsersChart { get; set; } = new();
}

public class DashboardChartDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class AdminNotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SystemHealthDto
{
    public bool IsHealthy { get; set; }
    public List<ServiceHealthDto> Services { get; set; } = new();
    public DatabaseHealthDto Database { get; set; } = new();
    public ExternalServiceHealthDto ExternalServices { get; set; } = new();
}

public class ServiceHealthDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public double ResponseTime { get; set; }
    public DateTime LastCheck { get; set; }
}

public class DatabaseHealthDto
{
    public bool IsHealthy { get; set; }
    public double ResponseTime { get; set; }
    public int ConnectionCount { get; set; }
}

public class ExternalServiceHealthDto
{
    public bool StripeHealthy { get; set; }
    public bool RedisHealthy { get; set; }
    public bool KafkaHealthy { get; set; }
    public bool QdrantHealthy { get; set; }
}

public class AdminAuditLogDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}