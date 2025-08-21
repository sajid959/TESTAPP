namespace DSAGrind.Events;

public class PaymentInitiatedEvent
{
    public string PaymentId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
}

public class PaymentCompletedEvent
{
    public string PaymentId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderPaymentId { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

public class PaymentFailedEvent
{
    public string PaymentId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string FailureReason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;
}

public class SubscriptionCreatedEvent
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderSubscriptionId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SubscriptionUpdatedEvent
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? OldPlan { get; set; }
    public string? NewPlan { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class SubscriptionCancelledEvent
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string CancellationReason { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveEndDate { get; set; }
}

public class RefundProcessedEvent
{
    public string PaymentId { get; set; } = string.Empty;
    public string RefundId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}