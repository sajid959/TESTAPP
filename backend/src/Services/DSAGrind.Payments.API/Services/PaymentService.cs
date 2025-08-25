using DSAGrind.Common.Repositories;
using DSAGrind.Models.Entities;
using DSAGrind.Models.DTOs;
using DSAGrind.Common.Services;
using Stripe;
using AutoMapper;

namespace DSAGrind.Payments.API.Services;

public class PaymentService : IPaymentService
{
    private readonly IMongoRepository<Payment> _paymentRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;
    private readonly PaymentIntentService _paymentIntentService;

    public PaymentService(
        IMongoRepository<Payment> paymentRepository,
        IEventPublisher eventPublisher,
        IMapper mapper,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
        _logger = logger;
        _paymentIntentService = new PaymentIntentService();
    }

    public async Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create Stripe PaymentIntent
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100), // Convert to cents
                Currency = request.Currency,
                Description = "Payment for DSAGrind subscription",
                Metadata = request.Metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>()
            };

            var paymentIntent = await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);

            // Create payment record
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = "pending",
                ProviderPaymentId = paymentIntent.Id,
                Metadata = request.Metadata?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) ?? new Dictionary<string, object>(),
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.CreateAsync(payment, cancellationToken);

            // Publish event
            await _eventPublisher.PublishAsync("payment.created", new { PaymentId = payment.Id, UserId = userId, Amount = request.Amount }, cancellationToken);

            return new PaymentIntentDto
            {
                Id = payment.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = "pending"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent for user {UserId}", userId);
            throw;
        }
    }

    public async Task<PaymentDto?> GetPaymentAsync(string paymentId, string userId, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null || payment.UserId != userId) return null;

        return new PaymentDto
        {
            Id = payment.Id,
            UserId = payment.UserId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            StripePaymentId = payment.ProviderPaymentId,
            CreatedAt = payment.CreatedAt
        };
    }

    public async Task<List<PaymentDto>> GetUserPaymentsAsync(string userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetManyAsync(
            p => p.UserId == userId,
            cancellationToken);

        // Simple pagination without using page/pageSize parameters in GetManyAsync for now
        var pagedPayments = payments.Skip((page - 1) * pageSize).Take(pageSize);
        
        return pagedPayments.Select(p => new PaymentDto
        {
            Id = p.Id,
            UserId = p.UserId,
            Amount = p.Amount,
            Currency = p.Currency,
            Status = p.Status,
            StripePaymentId = p.ProviderPaymentId,
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    public async Task<bool> ProcessWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, "your_webhook_secret");

            switch (stripeEvent.Type)
            {
                case Stripe.Events.PaymentIntentSucceeded:
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentSucceeded(paymentIntent?.Id, cancellationToken);
                    break;

                case Stripe.Events.PaymentIntentPaymentFailed:
                    var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentFailed(failedPaymentIntent?.Id, cancellationToken);
                    break;

                default:
                    _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return false;
        }
    }

    public async Task<RefundDto> RefundPaymentAsync(string paymentId, decimal? amount, string reason, string adminUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment == null) throw new ArgumentException("Payment not found");

            var refundService = new RefundService();
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = payment.ProviderPaymentId,
                Amount = amount.HasValue ? (long)(amount.Value * 100) : null,
                Reason = reason
            };

            var refund = await refundService.CreateAsync(refundOptions, cancellationToken: cancellationToken);

            // Update payment record
            payment.RefundInfo = new RefundInfo
            {
                RefundId = refund.Id,
                Amount = (decimal)refund.Amount / 100,
                Reason = reason,
                ProcessedAt = DateTime.UtcNow,
                Status = "succeeded"
            };
            payment.Status = "refunded";
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment.Id, payment, cancellationToken);

            // Publish event
            await _eventPublisher.PublishAsync("payment.refunded", new { PaymentId = paymentId, RefundAmount = payment.RefundInfo.Amount, AdminUserId = adminUserId }, cancellationToken);

            return new RefundDto
            {
                Id = refund.Id,
                PaymentId = paymentId,
                Amount = payment.RefundInfo.Amount,
                Reason = reason,
                Status = "succeeded",
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentId}", paymentId);
            throw;
        }
    }

    private async Task HandlePaymentSucceeded(string? stripePaymentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(stripePaymentId)) return;

        var payment = await _paymentRepository.GetAsync(p => p.ProviderPaymentId == stripePaymentId, cancellationToken);
        if (payment != null)
        {
            payment.Status = "succeeded";
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment.Id, payment, cancellationToken);

            await _eventPublisher.PublishAsync("payment.succeeded", new { PaymentId = payment.Id, UserId = payment.UserId }, cancellationToken);
        }
    }

    private async Task HandlePaymentFailed(string? stripePaymentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(stripePaymentId)) return;

        var payment = await _paymentRepository.GetAsync(p => p.ProviderPaymentId == stripePaymentId, cancellationToken);
        if (payment != null)
        {
            payment.Status = "failed";
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment.Id, payment, cancellationToken);

            await _eventPublisher.PublishAsync("payment.failed", new { PaymentId = payment.Id, UserId = payment.UserId }, cancellationToken);
        }
    }
}
