using DSAGrind.Common.Repositories;
using DSAGrind.Models.Entities;
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
                Description = request.Description,
                Metadata = request.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
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
                Description = request.Description,
                ProviderPaymentId = paymentIntent.Id,
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

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<List<PaymentDto>> GetUserPaymentsAsync(string userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetManyAsync(
            p => p.UserId == userId,
            page: page,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        return _mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<bool> ProcessWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, "your_webhook_secret");

            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentSucceeded(paymentIntent?.Id, cancellationToken);
                    break;

                case Events.PaymentIntentPaymentFailed:
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
                PaymentIntent = payment.StripePaymentId,
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

            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            // Publish event
            await _eventPublisher.PublishAsync("payment.refunded", new { PaymentId = paymentId, RefundAmount = payment.RefundDetails.Amount, AdminUserId = adminUserId }, cancellationToken);

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
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

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
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            await _eventPublisher.PublishAsync("payment.failed", new { PaymentId = payment.Id, UserId = payment.UserId }, cancellationToken);
        }
    }
}

public class SubscriptionService : ISubscriptionService
{
    private readonly IMongoRepository<Subscription> _subscriptionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly Stripe.SubscriptionService _stripeSubscriptionService;

    public SubscriptionService(
        IMongoRepository<Subscription> subscriptionRepository,
        IEventPublisher eventPublisher,
        IMapper mapper,
        ILogger<SubscriptionService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
        _logger = logger;
        _stripeSubscriptionService = new Stripe.SubscriptionService();
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create Stripe subscription
            var options = new SubscriptionCreateOptions
            {
                Customer = userId, // In real implementation, get Stripe customer ID
                Items = new List<SubscriptionItemOptions>
                {
                    new() { Price = request.PlanId }
                },
                DefaultPaymentMethod = request.PaymentMethodId,
                Expand = new List<string> { "latest_invoice.payment_intent" }
            };

            var stripeSubscription = await _stripeSubscriptionService.CreateAsync(options, cancellationToken: cancellationToken);

            // Create subscription record
            var subscription = new Subscription
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Plan = request.PlanId,
                Status = stripeSubscription.Status,
                StartDate = stripeSubscription.CurrentPeriodStart,
                EndDate = stripeSubscription.CurrentPeriodEnd,
                ProviderSubscriptionId = stripeSubscription.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _subscriptionRepository.CreateAsync(subscription, cancellationToken);

            // Publish event
            await _eventPublisher.PublishAsync("subscription.created", new { SubscriptionId = subscription.Id, UserId = userId, PlanId = request.PlanId }, cancellationToken);

            return _mapper.Map<SubscriptionDto>(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for user {UserId}", userId);
            throw;
        }
    }

    public async Task<SubscriptionDto?> GetUserSubscriptionAsync(string userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetAsync(s => s.UserId == userId, cancellationToken);
        return subscription == null ? null : _mapper.Map<SubscriptionDto>(subscription);
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null || subscription.UserId != userId) return false;

            // Cancel Stripe subscription
            await _stripeSubscriptionService.CancelAsync(subscription.ProviderSubscriptionId, cancellationToken: cancellationToken);

            // Update subscription record
            subscription.Status = "canceled";
            subscription.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            // Publish event
            await _eventPublisher.PublishAsync("subscription.canceled", new { SubscriptionId = subscriptionId, UserId = userId }, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<SubscriptionDto> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null || subscription.UserId != userId) throw new ArgumentException("Subscription not found");

            var options = new SubscriptionUpdateOptions();

            if (!string.IsNullOrEmpty(request.PlanId))
            {
                options.Items = new List<SubscriptionItemOptions>
                {
                    new() { Price = request.PlanId }
                };
                subscription.Plan = request.PlanId;
            }

            if (request.CancelAtPeriodEnd.HasValue)
            {
                options.CancelAtPeriodEnd = request.CancelAtPeriodEnd.Value;
                subscription.CancelledAt = request.CancelAtPeriodEnd.Value ? DateTime.UtcNow : null;
            }

            // Update Stripe subscription
            await _stripeSubscriptionService.UpdateAsync(subscription.ProviderSubscriptionId, options, cancellationToken: cancellationToken);

            // Update subscription record
            subscription.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            // Publish event
            await _eventPublisher.PublishAsync("subscription.updated", new { SubscriptionId = subscriptionId, UserId = userId }, cancellationToken);

            return _mapper.Map<SubscriptionDto>(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync(CancellationToken cancellationToken = default)
    {
        // Mock subscription plans - in real implementation, fetch from Stripe or database
        return await Task.FromResult(new List<SubscriptionPlanDto>
        {
            new()
            {
                Id = "plan_basic",
                Name = "Basic",
                Description = "Access to basic problems and features",
                Price = 9.99m,
                Currency = "usd",
                Interval = "month",
                Features = new List<string> { "100 problems", "Basic IDE", "Community support" },
                IsPopular = false,
                StripePriceId = "price_basic_monthly"
            },
            new()
            {
                Id = "plan_premium",
                Name = "Premium",
                Description = "Full access to all features",
                Price = 19.99m,
                Currency = "usd",
                Interval = "month",
                Features = new List<string> { "Unlimited problems", "Advanced IDE", "AI assistance", "Priority support" },
                IsPopular = true,
                StripePriceId = "price_premium_monthly"
            },
            new()
            {
                Id = "plan_annual",
                Name = "Annual Premium",
                Description = "Premium features with annual billing",
                Price = 199.99m,
                Currency = "usd",
                Interval = "year",
                Features = new List<string> { "Everything in Premium", "2 months free", "Advanced analytics" },
                IsPopular = false,
                StripePriceId = "price_premium_annual"
            }
        });
    }
}