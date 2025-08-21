using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using DSAGrind.Common.Services;

namespace DSAGrind.Auth.API.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        return await SendEmailAsync(new List<string> { to }, subject, body, isHtml, cancellationToken);
    }

    public async Task<bool> SendEmailAsync(List<string> to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        return await SendEmailAsync(string.Join(",", to), null, null, subject, body, isHtml, cancellationToken);
    }

    public async Task<bool> SendEmailAsync(string to, string? cc, string? bcc, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        if (!_emailSettings.IsEnabled)
        {
            _logger.LogWarning("Email sending is disabled. Skipping email to {To} with subject: {Subject}", to, subject);
            return true;
        }

        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.UseSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                Timeout = _emailSettings.TimeoutSeconds * 1000
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            // Add recipients
            foreach (var recipient in to.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                message.To.Add(recipient.Trim());
            }

            if (!string.IsNullOrEmpty(cc))
            {
                foreach (var ccRecipient in cc.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    message.CC.Add(ccRecipient.Trim());
                }
            }

            if (!string.IsNullOrEmpty(bcc))
            {
                foreach (var bccRecipient in bcc.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    message.Bcc.Add(bccRecipient.Trim());
                }
            }

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent successfully to {To} with subject: {Subject}", to, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject: {Subject}", to, subject);
            return false;
        }
    }

    public async Task<bool> SendEmailVerificationAsync(string to, string username, string verificationToken, CancellationToken cancellationToken = default)
    {
        var verificationUrl = $"{_emailSettings.BaseUrl}/verify-email?token={verificationToken}";
        
        var subject = "Verify Your Email - DSAGrind";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Welcome to DSAGrind!</h1>
        </div>
        <div class=""content"">
            <h2>Hi {username},</h2>
            <p>Thank you for joining DSAGrind! To complete your registration, please verify your email address by clicking the button below:</p>
            <a href=""{verificationUrl}"" class=""button"">Verify Email Address</a>
            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; background: #fff; padding: 10px; border-radius: 5px;"">{verificationUrl}</p>
            <p>This verification link will expire in 24 hours for security reasons.</p>
            <p>If you didn't create an account with DSAGrind, please ignore this email.</p>
            <p>Happy coding!<br>The DSAGrind Team</p>
        </div>
        <div class=""footer"">
            <p>© 2024 DSAGrind. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task<bool> SendPasswordResetAsync(string to, string username, string resetToken, CancellationToken cancellationToken = default)
    {
        var resetUrl = $"{_emailSettings.BaseUrl}/reset-password?token={resetToken}";
        
        var subject = "Reset Your Password - DSAGrind";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; color: #856404; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Password Reset Request</h1>
        </div>
        <div class=""content"">
            <h2>Hi {username},</h2>
            <p>We received a request to reset your password for your DSAGrind account.</p>
            <p>Click the button below to reset your password:</p>
            <a href=""{resetUrl}"" class=""button"">Reset Password</a>
            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; background: #fff; padding: 10px; border-radius: 5px;"">{resetUrl}</p>
            <div class=""warning"">
                <strong>Security Notice:</strong>
                <ul>
                    <li>This link will expire in 1 hour for security reasons</li>
                    <li>If you didn't request this reset, please ignore this email</li>
                    <li>Your password will not be changed unless you click the link above</li>
                </ul>
            </div>
            <p>For security reasons, we recommend choosing a strong password that includes:</p>
            <ul>
                <li>At least 8 characters</li>
                <li>A mix of uppercase and lowercase letters</li>
                <li>Numbers and special characters</li>
            </ul>
            <p>Best regards,<br>The DSAGrind Team</p>
        </div>
        <div class=""footer"">
            <p>© 2024 DSAGrind. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task<bool> SendWelcomeEmailAsync(string to, string username, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to DSAGrind - Let's Start Coding!";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .features {{ background: white; padding: 20px; border-radius: 5px; margin: 20px 0; }}
        .feature {{ margin: 15px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎉 Welcome to DSAGrind!</h1>
        </div>
        <div class=""content"">
            <h2>Hi {username},</h2>
            <p>Congratulations! Your email has been verified and you're now ready to start your coding journey with DSAGrind.</p>
            
            <div class=""features"">
                <h3>What you can do now:</h3>
                <div class=""feature"">📚 <strong>Browse Problems:</strong> Explore our curated collection of coding challenges</div>
                <div class=""feature"">💻 <strong>Online IDE:</strong> Code directly in your browser with our powerful editor</div>
                <div class=""feature"">🤖 <strong>AI Assistance:</strong> Get hints and explanations when you're stuck</div>
                <div class=""feature"">📊 <strong>Track Progress:</strong> Monitor your growth and see your statistics</div>
                <div class=""feature"">🏆 <strong>Compete:</strong> Join contests and challenge other developers</div>
            </div>

            <a href=""{_emailSettings.BaseUrl}/problems"" class=""button"">Start Solving Problems</a>
            
            <p>Need help getting started? Check out our <a href=""{_emailSettings.BaseUrl}/help"">Help Center</a> or reach out to our support team.</p>
            
            <p>Happy coding and welcome to the community!<br>The DSAGrind Team</p>
        </div>
        <div class=""footer"">
            <p>© 2024 DSAGrind. All rights reserved.</p>
            <p>Follow us on social media for tips and updates!</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task<bool> SendSubmissionResultAsync(string to, string username, string problemTitle, string status, CancellationToken cancellationToken = default)
    {
        var isAccepted = status.Equals("Accepted", StringComparison.OrdinalIgnoreCase);
        var emoji = isAccepted ? "🎉" : "💪";
        var statusColor = isAccepted ? "#28a745" : "#dc3545";
        
        var subject = $"Submission Result: {problemTitle} - {status}";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .status {{ background: {statusColor}; color: white; padding: 15px; text-align: center; border-radius: 5px; margin: 20px 0; font-size: 18px; font-weight: bold; }}
        .button {{ background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>{emoji} Submission Result</h1>
        </div>
        <div class=""content"">
            <h2>Hi {username},</h2>
            <p>Your submission for <strong>{problemTitle}</strong> has been processed.</p>
            
            <div class=""status"">{status}</div>
            
            {(isAccepted ? 
                "<p>🎊 Congratulations! Your solution is correct. Great job solving this problem!</p>" :
                "<p>Don't give up! Every attempt brings you closer to the solution. Review your code and try again.</p>")}
            
            <a href=""{_emailSettings.BaseUrl}/problems/{problemTitle.ToLower().Replace(" ", "-")}"" class=""button"">View Problem Details</a>
            
            <p>Keep practicing and happy coding!<br>The DSAGrind Team</p>
        </div>
        <div class=""footer"">
            <p>© 2024 DSAGrind. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task<bool> SendPaymentConfirmationAsync(string to, string username, decimal amount, string plan, CancellationToken cancellationToken = default)
    {
        var subject = "Payment Confirmation - DSAGrind Premium";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .success {{ background: #d4edda; border: 1px solid #c3e6cb; color: #155724; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .details {{ background: white; padding: 20px; border-radius: 5px; margin: 20px 0; }}
        .button {{ background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎉 Payment Successful!</h1>
        </div>
        <div class=""content"">
            <h2>Hi {username},</h2>
            
            <div class=""success"">
                <strong>✅ Your payment has been processed successfully!</strong>
            </div>
            
            <p>Thank you for upgrading to DSAGrind Premium. You now have access to all premium features!</p>
            
            <div class=""details"">
                <h3>Payment Details:</h3>
                <p><strong>Plan:</strong> {plan}</p>
                <p><strong>Amount:</strong> ${amount:F2}</p>
                <p><strong>Date:</strong> {DateTime.UtcNow:MMMM dd, yyyy}</p>
            </div>
            
            <h3>Your Premium Features:</h3>
            <ul>
                <li>🔓 Unlimited access to all problems</li>
                <li>🤖 Advanced AI assistance and hints</li>
                <li>📊 Detailed analytics and progress tracking</li>
                <li>⚡ Priority code execution</li>
                <li>🎨 Custom themes and editor features</li>
                <li>💬 Premium support</li>
            </ul>
            
            <a href=""{_emailSettings.BaseUrl}/dashboard"" class=""button"">Access Your Dashboard</a>
            
            <p>If you have any questions about your subscription or need assistance, please don't hesitate to contact our support team.</p>
            
            <p>Thank you for supporting DSAGrind!<br>The DSAGrind Team</p>
        </div>
        <div class=""footer"">
            <p>© 2024 DSAGrind. All rights reserved.</p>
            <p>Questions? Contact us at support@dsagrind.com</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(to, subject, body, true, cancellationToken);
    }
}