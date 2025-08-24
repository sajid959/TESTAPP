using System.ComponentModel.DataAnnotations;

namespace DSAGrind.Models.DTOs;

// Payment DTOs
public class CreatePaymentIntentRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    public string Currency { get; set; } = "usd";
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class CreatePaymentRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    public string Currency { get; set; } = "usd";
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class ConfirmPaymentRequestDto
{
    [Required]
    public string PaymentIntentId { get; set; } = string.Empty;
    
    public string? PaymentMethodId { get; set; }
    public bool SavePaymentMethod { get; set; } = false;
}

public class PaymentIntentDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PaymentResultDto
{
    public string PaymentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class PaymentDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StripePaymentId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public RefundDetailsDto? RefundDetails { get; set; }
}

public class RefundDto
{
    public string Id { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RefundDetailsDto
{
    public string RefundId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime RefundedAt { get; set; }
}

// Subscription DTOs
public class CreateSubscriptionRequestDto
{
    [Required]
    public string PlanId { get; set; } = string.Empty;
    
    [Required]
    public string PaymentMethodId { get; set; } = string.Empty;
}

public class UpdateSubscriptionRequestDto
{
    public string? PlanId { get; set; }
    public bool? CancelAtPeriodEnd { get; set; }
}

public class SubscriptionDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public string StripeSubscriptionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public SubscriptionPlanDto? Plan { get; set; }
}

public class SubscriptionPlanDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Interval { get; set; } = string.Empty; // monthly, yearly
    public List<string> Features { get; set; } = new();
    public bool IsPopular { get; set; }
    public string StripePriceId { get; set; } = string.Empty;
}

// Admin DTOs (extending Auth DTOs)
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
    public string Type { get; set; } = string.Empty; // info, warning, error, success
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