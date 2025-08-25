using DSAGrind.Models.DTOs;

namespace DSAGrind.Payments.API.Services;

public interface ISubscriptionService
{
    Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<SubscriptionDto?> GetUserSubscriptionAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> CancelSubscriptionAsync(string subscriptionId, string userId, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionRequestDto request, string userId, CancellationToken cancellationToken = default);
    Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync(CancellationToken cancellationToken = default);
}