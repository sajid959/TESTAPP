using DSAGrind.Common.Extensions;
using DSAGrind.Models.Settings;
using DSAGrind.Payments.API.Services;
using Serilog;
using Stripe;
using System.Reflection;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5006); // HTTP only
});
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

// Add JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<DSAGrind.Common.Configuration.JwtSettings>();
        options.Authority = builder.Configuration.GetValue<string>("Auth:Authority") ?? "http://localhost:8080";
        options.Audience = jwtSettings?.Audience ?? "DSAGrind-Users";
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization();
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

// Get allowed origins from environment
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
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
    var port = builder.Configuration.GetValue<string>("Payments:Port") ?? "5006";
    var host = builder.Configuration.GetValue<string>("Payments:Host") ?? "0.0.0.0";
    //var useHttps = builder.Configuration.GetValue<bool>("Payments:UseHttps", true);
    //var protocol = useHttps ? "https" : "http";
    //var url = $"{protocol}://{host}:{port}";
    var url = $"http://{host}:{port}";
    Log.Information($"Starting DSAGrind Payments API on {url}");
    app.Run(url);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Payments API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}