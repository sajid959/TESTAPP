using DSAGrind.Common.Repositories;
using DSAGrind.Models.Entities;
using DSAGrind.Models.DTOs;
using DSAGrind.Common.Services;
using DSAGrind.Models.Settings;
using AutoMapper;
using Microsoft.Extensions.Options;
using Stripe;

namespace DSAGrind.Payments.API.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IMongoRepository<DSAGrind.Models.Entities.Subscription> _subscriptionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly PaymentSettings _paymentSettings;
    private readonly Stripe.SubscriptionService? _stripeSubscriptionService;
    private readonly PriceService? _stripePriceService;

    public SubscriptionService(
        IMongoRepository<DSAGrind.Models.Entities.Subscription> subscriptionRepository,
        IEventPublisher eventPublisher,
        IMapper mapper,
        ILogger<SubscriptionService> logger,
        IOptions<PaymentSettings> paymentSettings)
    {
        _subscriptionRepository = subscriptionRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
        _logger = logger;
        _paymentSettings = paymentSettings.Value;
        
        if (_paymentSettings.EnableStripeIntegration)
        {
            _stripeSubscriptionService = new Stripe.SubscriptionService();
            _stripePriceService = new PriceService();
        }
        else
        {
            _stripeSubscriptionService = null;
            _stripePriceService = null;
        }
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_paymentSettings.UseMockData || !_paymentSettings.EnableStripeIntegration)
            {
                // Mock subscription creation
                _logger.LogInformation("Creating mock subscription for user {UserId} with plan {PlanId}", userId, request.PlanId);
                
                await Task.Delay(100, cancellationToken); // Simulate API delay

                var subscription = new SubscriptionDto
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    PlanId = request.PlanId,
                    Status = "active",
                    CurrentPeriodStart = DateTime.UtcNow,
                    CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1),
                    CancelAtPeriodEnd = false,
                    StripeSubscriptionId = "sub_mock_" + Guid.NewGuid().ToString()[..8],
                    CreatedAt = DateTime.UtcNow
                };

                await _eventPublisher.PublishAsync("subscription.created", new { SubscriptionId = subscription.Id, UserId = userId }, cancellationToken);
                return subscription;
            }
            else
            {
                // Real Stripe integration
                _logger.LogInformation("Creating real Stripe subscription for user {UserId} with plan {PlanId}", userId, request.PlanId);
                
                var options = new SubscriptionCreateOptions
                {
                    Customer = userId, // In production, this should be the Stripe customer ID
                    Items = new List<SubscriptionItemOptions>
                    {
                        new() { Price = request.PlanId }
                    },
                    DefaultPaymentMethod = request.PaymentMethodId,
                    Expand = new List<string> { "latest_invoice.payment_intent" }
                };

                var stripeSubscription = await _stripeSubscriptionService!.CreateAsync(options, cancellationToken: cancellationToken);

                var subscription = new SubscriptionDto
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    PlanId = request.PlanId,
                    Status = stripeSubscription.Status,
                    CurrentPeriodStart = stripeSubscription.CurrentPeriodStart,
                    CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd,
                    CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd,
                    StripeSubscriptionId = stripeSubscription.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _eventPublisher.PublishAsync("subscription.created", new { SubscriptionId = subscription.Id, UserId = userId }, cancellationToken);
                return subscription;
            }
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
            if (_paymentSettings.UseMockData || !_paymentSettings.EnableStripeIntegration)
            {
                // Mock implementation
                _logger.LogInformation("Getting mock subscription for user {UserId}", userId);
                
                await Task.Delay(50, cancellationToken);
                
                return new SubscriptionDto
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    PlanId = "price_premium_monthly",
                    Status = "active",
                    CurrentPeriodStart = DateTime.UtcNow.AddDays(-15),
                    CurrentPeriodEnd = DateTime.UtcNow.AddDays(15),
                    CancelAtPeriodEnd = false,
                    StripeSubscriptionId = "sub_mock_" + Guid.NewGuid().ToString()[..8],
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                };
            }
            else
            {
                // Real database query for production
                var subscription = await _subscriptionRepository.GetAsync(s => s.UserId == userId, cancellationToken);
                return subscription != null ? _mapper.Map<SubscriptionDto>(subscription) : null;
            }
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
            if (_paymentSettings.UseMockData || !_paymentSettings.EnableStripeIntegration)
            {
                // Mock subscription cancellation
                _logger.LogInformation("Cancelling mock subscription {SubscriptionId} for user {UserId}", subscriptionId, userId);
                await Task.Delay(100, cancellationToken);
            }
            else
            {
                // Real Stripe cancellation
                _logger.LogInformation("Cancelling Stripe subscription {SubscriptionId} for user {UserId}", subscriptionId, userId);
                var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = true };
                await _stripeSubscriptionService!.UpdateAsync(subscriptionId, options, cancellationToken: cancellationToken);
            }

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
            if (_paymentSettings.UseMockData || !_paymentSettings.EnableStripeIntegration)
            {
                // Mock subscription update
                _logger.LogInformation("Updating mock subscription {SubscriptionId} for user {UserId}", subscriptionId, userId);
                await Task.Delay(100, cancellationToken);

                var subscription = new SubscriptionDto
                {
                    Id = subscriptionId,
                    UserId = userId,
                    PlanId = request.PlanId ?? "price_premium_monthly",
                    Status = "active",
                    CurrentPeriodStart = DateTime.UtcNow.AddDays(-15),
                    CurrentPeriodEnd = DateTime.UtcNow.AddDays(15),
                    CancelAtPeriodEnd = request.CancelAtPeriodEnd ?? false,
                    StripeSubscriptionId = "sub_mock_" + Guid.NewGuid().ToString()[..8],
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                };

                await _eventPublisher.PublishAsync("subscription.updated", new { SubscriptionId = subscriptionId, UserId = userId }, cancellationToken);
                return subscription;
            }
            else
            {
                // Real Stripe update
                _logger.LogInformation("Updating Stripe subscription {SubscriptionId} for user {UserId}", subscriptionId, userId);
                
                var options = new SubscriptionUpdateOptions();
                if (!string.IsNullOrEmpty(request.PlanId))
                {
                    options.Items = new List<SubscriptionItemOptions>
                    {
                        new() { Price = request.PlanId }
                    };
                }
                if (request.CancelAtPeriodEnd.HasValue)
                {
                    options.CancelAtPeriodEnd = request.CancelAtPeriodEnd.Value;
                }

                var stripeSubscription = await _stripeSubscriptionService!.UpdateAsync(subscriptionId, options, cancellationToken: cancellationToken);

                var subscription = new SubscriptionDto
                {
                    Id = subscriptionId,
                    UserId = userId,
                    PlanId = request.PlanId ?? stripeSubscription.Items.Data.First().Price.Id,
                    Status = stripeSubscription.Status,
                    CurrentPeriodStart = stripeSubscription.CurrentPeriodStart,
                    CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd,
                    CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd,
                    StripeSubscriptionId = stripeSubscription.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _eventPublisher.PublishAsync("subscription.updated", new { SubscriptionId = subscriptionId, UserId = userId }, cancellationToken);
                return subscription;
            }
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
            if (_paymentSettings.UseMockData || !_paymentSettings.EnableStripeIntegration)
            {
                // Mock implementation
                _logger.LogInformation("Getting mock subscription plans");
                await Task.Delay(50, cancellationToken);
            }
            else
            {
                // Real Stripe plans could be fetched here in production
                _logger.LogInformation("Getting subscription plans from configuration");
            }
            
            // Return predefined plans (in production, these could come from Stripe or database)
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