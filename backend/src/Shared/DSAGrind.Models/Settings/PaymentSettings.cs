namespace DSAGrind.Models.Settings;

public class PaymentSettings
{
    public const string SectionName = "PaymentSettings";
    
    public bool UseMockData { get; set; } = true;
    public bool EnableStripeIntegration { get; set; } = false;
    public bool EnableWebhooks { get; set; } = false;
}