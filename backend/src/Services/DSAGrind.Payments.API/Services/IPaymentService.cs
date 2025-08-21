using DSAGrind.Models.DTOs;

namespace DSAGrind.Payments.API.Services;

public interface IPaymentService
{
    Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetPaymentAsync(string paymentId, string userId, CancellationToken cancellationToken = default);
    Task<List<PaymentDto>> GetUserPaymentsAsync(string userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> ProcessWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);
    Task<RefundDto> RefundPaymentAsync(string paymentId, decimal? amount, string reason, string adminUserId, CancellationToken cancellationToken = default);
}

public interface ISubscriptionService
{
    Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<SubscriptionDto?> GetUserSubscriptionAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> CancelSubscriptionAsync(string subscriptionId, string userId, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync(CancellationToken cancellationToken = default);
}

public class PaymentIntentDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
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
}

public class SubscriptionPlanDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Interval { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public bool IsPopular { get; set; }
    public string StripePriceId { get; set; } = string.Empty;
}

public class CreatePaymentRequestDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class CreateSubscriptionRequestDto
{
    public string PlanId { get; set; } = string.Empty;
    public string PaymentMethodId { get; set; } = string.Empty;
}

public class UpdateSubscriptionRequestDto
{
    public string? PlanId { get; set; }
    public bool? CancelAtPeriodEnd { get; set; }
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