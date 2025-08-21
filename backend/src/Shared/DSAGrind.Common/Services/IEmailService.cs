namespace DSAGrind.Common.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task<bool> SendEmailAsync(List<string> to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task<bool> SendEmailAsync(string to, string? cc, string? bcc, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task<bool> SendEmailVerificationAsync(string to, string username, string verificationToken, CancellationToken cancellationToken = default);
    Task<bool> SendPasswordResetAsync(string to, string username, string resetToken, CancellationToken cancellationToken = default);
    Task<bool> SendWelcomeEmailAsync(string to, string username, CancellationToken cancellationToken = default);
    Task<bool> SendSubmissionResultAsync(string to, string username, string problemTitle, string status, CancellationToken cancellationToken = default);
    Task<bool> SendPaymentConfirmationAsync(string to, string username, decimal amount, string plan, CancellationToken cancellationToken = default);
}

public class EmailTemplate
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string TextBody { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();
}

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public string BaseUrl { get; set; } = string.Empty; // For email verification links
}