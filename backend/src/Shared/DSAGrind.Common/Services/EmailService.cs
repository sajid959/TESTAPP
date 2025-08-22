using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DSAGrind.Common.Configuration;

namespace DSAGrind.Common.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(string email, string username, string verificationToken, CancellationToken cancellationToken = default)
    {
        var subject = "Verify your email address - DSAGrind";
        var verificationLink = $"http://localhost:3000/verify-email?token={verificationToken}";
        
        var htmlBody = $@"
            <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                        <h1 style='color: #343a40;'>Welcome to DSAGrind!</h1>
                        <p>Hi {username},</p>
                        <p>Thank you for signing up! Please verify your email address by clicking the button below:</p>
                        <a href='{verificationLink}' style='display: inline-block; padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0;'>Verify Email</a>
                        <p>If the button doesn't work, copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #6c757d;'>{verificationLink}</p>
                        <p style='margin-top: 30px; font-size: 12px; color: #6c757d;'>
                            This link will expire in 24 hours for security reasons.
                        </p>
                    </div>
                </body>
            </html>";

        var textBody = $@"
            Welcome to DSAGrind!
            
            Hi {username},
            
            Thank you for signing up! Please verify your email address by visiting:
            {verificationLink}
            
            This link will expire in 24 hours for security reasons.";

        await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string email, string username, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to DSAGrind - Start Your Coding Journey!";
        
        var htmlBody = $@"
            <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                        <h1 style='color: #28a745;'>🎉 Welcome to DSAGrind, {username}!</h1>
                        <p>Your email has been verified successfully!</p>
                        <p>You're now ready to start grinding data structures and algorithms:</p>
                        <ul style='text-align: left; max-width: 400px; margin: 20px auto;'>
                            <li>✅ Solve over 1000+ coding problems</li>
                            <li>✅ Track your progress with detailed analytics</li>
                            <li>✅ Get AI-powered hints when stuck</li>
                            <li>✅ Compete in weekly contests</li>
                            <li>✅ Join our community of developers</li>
                        </ul>
                        <a href='http://localhost:3000/problems' style='display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0;'>Start Coding Now!</a>
                        <p style='margin-top: 30px; font-size: 12px; color: #6c757d;'>
                            Happy coding! 🚀<br>
                            The DSAGrind Team
                        </p>
                    </div>
                </body>
            </html>";

        var textBody = $@"
            Welcome to DSAGrind, {username}!
            
            Your email has been verified successfully!
            
            You're now ready to start grinding data structures and algorithms:
            - Solve over 1000+ coding problems
            - Track your progress with detailed analytics
            - Get AI-powered hints when stuck
            - Compete in weekly contests
            - Join our community of developers
            
            Start coding: http://localhost:3000/problems
            
            Happy coding!
            The DSAGrind Team";

        await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string email, string username, string resetToken, CancellationToken cancellationToken = default)
    {
        var subject = "Reset your password - DSAGrind";
        var resetLink = $"http://localhost:3000/reset-password?token={resetToken}";
        
        var htmlBody = $@"
            <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                        <h1 style='color: #dc3545;'>🔒 Password Reset Request</h1>
                        <p>Hi {username},</p>
                        <p>We received a request to reset your password. Click the button below to create a new password:</p>
                        <a href='{resetLink}' style='display: inline-block; padding: 12px 24px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0;'>Reset Password</a>
                        <p>If the button doesn't work, copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #6c757d;'>{resetLink}</p>
                        <p style='margin-top: 30px; font-size: 12px; color: #6c757d;'>
                            This link will expire in 1 hour for security reasons.<br>
                            If you didn't request this reset, please ignore this email.
                        </p>
                    </div>
                </body>
            </html>";

        var textBody = $@"
            Password Reset Request
            
            Hi {username},
            
            We received a request to reset your password. Visit this link to create a new password:
            {resetLink}
            
            This link will expire in 1 hour for security reasons.
            If you didn't request this reset, please ignore this email.";

        await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                Timeout = _emailSettings.TimeoutSeconds * 1000
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromAddress, _emailSettings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            if (!string.IsNullOrEmpty(textBody))
            {
                var textView = AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain");
                mailMessage.AlternateViews.Add(textView);
            }

            await client.SendMailAsync(mailMessage, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} with subject: {Subject}", toEmail, subject);
            throw;
        }
    }

    public async Task SendBulkEmailAsync(List<string> toEmails, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
    {
        var tasks = toEmails.Select(email => SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken));
        await Task.WhenAll(tasks);
    }
}