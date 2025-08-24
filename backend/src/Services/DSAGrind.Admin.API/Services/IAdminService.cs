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

