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


