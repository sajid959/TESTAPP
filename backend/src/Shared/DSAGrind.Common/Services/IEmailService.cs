namespace DSAGrind.Common.Services;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string username, string verificationToken, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string email, string username, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string email, string username, string resetToken, CancellationToken cancellationToken = default);
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default);
    Task SendBulkEmailAsync(List<string> toEmails, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default);
}