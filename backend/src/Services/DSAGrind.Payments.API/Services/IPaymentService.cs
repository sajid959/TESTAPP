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

