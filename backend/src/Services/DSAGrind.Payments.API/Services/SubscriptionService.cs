using DSAGrind.Common.Repositories;
using DSAGrind.Models.Entities;
using DSAGrind.Models.DTOs;
using DSAGrind.Common.Services;
using AutoMapper;
using Stripe;

namespace DSAGrind.Payments.API.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IMongoRepository<DSAGrind.Models.Entities.Subscription> _subscriptionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly Stripe.SubscriptionService _stripeSubscriptionService;
    private readonly PriceService _stripePriceService;

    public SubscriptionService(
        IMongoRepository<DSAGrind.Models.Entities.Subscription> subscriptionRepository,
        IEventPublisher eventPublisher,
        IMapper mapper,
        ILogger<SubscriptionService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
        _logger = logger;
        _stripeSubscriptionService = new Stripe.SubscriptionService();
        _stripePriceService = new PriceService();
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create Stripe subscription
            var options = new SubscriptionCreateOptions
            {
                Customer = userId, // In real implementation, this should be the Stripe customer ID
                Items = new List<SubscriptionItemOptions>
                {
                    new()
                    {
                        Price = request.PlanId,
                    },
                },
                DefaultPaymentMethod = request.PaymentMethodId,
                Expand = new List<string> { "latest_invoice.payment_intent" }
            };

            var stripeSubscription = await _stripeSubscriptionService.CreateAsync(options, cancellationToken: cancellationToken);

            // Create subscription record in database
            var subscription = new SubscriptionDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                PlanId = request.PlanId,
                Status = stripeSubscription.Status,
                CurrentPeriodStart = DateTime.UnixEpoch.AddSeconds(stripeSubscription.CurrentPeriodStart),
                CurrentPeriodEnd = DateTime.UnixEpoch.AddSeconds(stripeSubscription.CurrentPeriodEnd),
                CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd,
                StripeSubscriptionId = stripeSubscription.Id,
                CreatedAt = DateTime.UtcNow
            };

            // Publish event
            await _eventPublisher.PublishAsync("subscription.created", new { SubscriptionId = subscription.Id, UserId = userId }, cancellationToken);

            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for user {UserId}", userId);
            throw;
        }
    }

    public async Task<SubscriptionDto?> GetUserSubscriptionAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // In real implementation, query from database
            // For now, return mock data
            return new SubscriptionDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                PlanId = "price_premium_monthly",
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow.AddDays(-15),
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(15),
                CancelAtPeriodEnd = false,
                StripeSubscriptionId = "sub_" + Guid.NewGuid().ToString()[..8],
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // In real implementation, find subscription by ID and verify user ownership
            // Cancel the Stripe subscription
            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true,
            };

            // await _stripeSubscriptionService.UpdateAsync(stripeSubscriptionId, options, cancellationToken: cancellationToken);

            // Publish event
            await _eventPublisher.PublishAsync("subscription.cancelled", new { SubscriptionId = subscriptionId, UserId = userId }, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription {SubscriptionId} for user {UserId}", subscriptionId, userId);
            return false;
        }
    }

    public async Task<SubscriptionDto> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // In real implementation, find subscription by ID and verify user ownership
            var options = new SubscriptionUpdateOptions();

            if (!string.IsNullOrEmpty(request.PlanId))
            {
                options.Items = new List<SubscriptionItemOptions>
                {
                    new()
                    {
                        Price = request.PlanId,
                    },
                };
            }

            if (request.CancelAtPeriodEnd.HasValue)
            {
                options.CancelAtPeriodEnd = request.CancelAtPeriodEnd.Value;
            }

            // var stripeSubscription = await _stripeSubscriptionService.UpdateAsync(stripeSubscriptionId, options, cancellationToken: cancellationToken);

            // Update subscription record in database and return
            var subscription = new SubscriptionDto
            {
                Id = subscriptionId,
                UserId = userId,
                PlanId = request.PlanId ?? "price_premium_monthly",
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow.AddDays(-15),
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(15),
                CancelAtPeriodEnd = request.CancelAtPeriodEnd ?? false,
                StripeSubscriptionId = "sub_" + Guid.NewGuid().ToString()[..8],
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            };

            // Publish event
            await _eventPublisher.PublishAsync("subscription.updated", new { SubscriptionId = subscriptionId, UserId = userId }, cancellationToken);

            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId} for user {UserId}", subscriptionId, userId);
            throw;
        }
    }

    public async Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // In real implementation, fetch from Stripe and/or database
            return new List<SubscriptionPlanDto>
            {
                new()
                {
                    Id = "price_basic_monthly",
                    Name = "Basic",
                    Description = "Essential features for coding practice",
                    Price = 9.99m,
                    Currency = "usd",
                    Interval = "monthly",
                    Features = new List<string>
                    {
                        "Access to all problems",
                        "Basic AI hints",
                        "Progress tracking"
                    },
                    IsPopular = false,
                    StripePriceId = "price_basic_monthly"
                },
                new()
                {
                    Id = "price_premium_monthly",
                    Name = "Premium",
                    Description = "Advanced features for serious developers",
                    Price = 19.99m,
                    Currency = "usd",
                    Interval = "monthly",
                    Features = new List<string>
                    {
                        "Everything in Basic",
                        "Advanced AI assistance",
                        "Code review",
                        "Interview prep",
                        "Priority support"
                    },
                    IsPopular = true,
                    StripePriceId = "price_premium_monthly"
                },
                new()
                {
                    Id = "price_premium_yearly",
                    Name = "Premium Yearly",
                    Description = "Premium features with yearly savings",
                    Price = 199.99m,
                    Currency = "usd",
                    Interval = "yearly",
                    Features = new List<string>
                    {
                        "Everything in Premium",
                        "2 months free",
                        "Exclusive content",
                        "Personal mentor sessions"
                    },
                    IsPopular = false,
                    StripePriceId = "price_premium_yearly"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plans");
            throw;
        }
    }
}