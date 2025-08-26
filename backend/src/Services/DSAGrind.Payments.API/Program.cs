using DSAGrind.Common.Extensions;
using DSAGrind.Payments.API.Services;
using DSAGrind.Models.Settings;
using Serilog;
using System.Reflection;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/payments-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCommonServices(builder.Configuration);
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Configure settings
builder.Services.Configure<PaymentSettings>(builder.Configuration.GetSection(PaymentSettings.SectionName));

// Configure Stripe conditionally
var paymentSettings = builder.Configuration.GetSection(PaymentSettings.SectionName).Get<PaymentSettings>();
if (paymentSettings?.EnableStripeIntegration == true)
{
    var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
    if (!string.IsNullOrEmpty(stripeSecretKey))
    {
        StripeConfiguration.ApiKey = stripeSecretKey;
    }
}

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<DSAGrind.Payments.API.Services.ISubscriptionService, DSAGrind.Payments.API.Services.SubscriptionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => new { status = "healthy", service = "DSAGrind.Payments.API", timestamp = DateTime.UtcNow });

try
{
    Log.Information("Starting DSAGrind Payments API on port 5006");
    app.Run("https://localhost:5006");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Payments API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}