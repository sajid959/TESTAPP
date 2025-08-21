using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DSAGrind.Models.Entities;

public class Payment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("subscriptionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? SubscriptionId { get; set; }

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "USD";

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty; // pending, completed, failed, refunded

    [BsonElement("provider")]
    public string Provider { get; set; } = string.Empty; // stripe, razorpay

    [BsonElement("providerPaymentId")]
    public string ProviderPaymentId { get; set; } = string.Empty;

    [BsonElement("paymentMethod")]
    public PaymentMethod PaymentMethod { get; set; } = new();

    [BsonElement("billing")]
    public BillingInfo Billing { get; set; } = new();

    [BsonElement("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [BsonElement("refundInfo")]
    public RefundInfo? RefundInfo { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Subscription
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("plan")]
    public string Plan { get; set; } = string.Empty; // free, premium_monthly, premium_annual

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty; // active, cancelled, expired, suspended

    [BsonElement("provider")]
    public string Provider { get; set; } = string.Empty;

    [BsonElement("providerSubscriptionId")]
    public string ProviderSubscriptionId { get; set; } = string.Empty;

    [BsonElement("startDate")]
    public DateTime StartDate { get; set; }

    [BsonElement("endDate")]
    public DateTime? EndDate { get; set; }

    [BsonElement("nextBillingDate")]
    public DateTime? NextBillingDate { get; set; }

    [BsonElement("cancelledAt")]
    public DateTime? CancelledAt { get; set; }

    [BsonElement("features")]
    public SubscriptionFeatures Features { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class PaymentMethod
{
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // card, upi, wallet

    [BsonElement("last4")]
    public string? Last4 { get; set; }

    [BsonElement("brand")]
    public string? Brand { get; set; }

    [BsonElement("expiryMonth")]
    public int? ExpiryMonth { get; set; }

    [BsonElement("expiryYear")]
    public int? ExpiryYear { get; set; }
}

public class BillingInfo
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("address")]
    public Address Address { get; set; } = new();
}

public class Address
{
    [BsonElement("line1")]
    public string Line1 { get; set; } = string.Empty;

    [BsonElement("line2")]
    public string? Line2 { get; set; }

    [BsonElement("city")]
    public string City { get; set; } = string.Empty;

    [BsonElement("state")]
    public string State { get; set; } = string.Empty;

    [BsonElement("postalCode")]
    public string PostalCode { get; set; } = string.Empty;

    [BsonElement("country")]
    public string Country { get; set; } = string.Empty;
}

public class RefundInfo
{
    [BsonElement("refundId")]
    public string RefundId { get; set; } = string.Empty;

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("reason")]
    public string Reason { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;

    [BsonElement("processedAt")]
    public DateTime ProcessedAt { get; set; }
}

public class SubscriptionFeatures
{
    [BsonElement("unlimitedProblems")]
    public bool UnlimitedProblems { get; set; } = false;

    [BsonElement("aiAssistance")]
    public bool AiAssistance { get; set; } = false;

    [BsonElement("premiumSupport")]
    public bool PremiumSupport { get; set; } = false;

    [BsonElement("advancedAnalytics")]
    public bool AdvancedAnalytics { get; set; } = false;

    [BsonElement("customThemes")]
    public bool CustomThemes { get; set; } = false;

    [BsonElement("priorityExecution")]
    public bool PriorityExecution { get; set; } = false;
}