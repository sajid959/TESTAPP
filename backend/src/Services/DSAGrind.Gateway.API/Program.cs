using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add controllers
builder.Services.AddControllers();

// Add reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://0.0.0.0:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Rate limiter disabled for now
app.UseCors("AllowFrontend");

// Map controllers first
app.MapControllers();

// Map reverse proxy
app.MapReverseProxy();

app.MapGet("/health", () => new { status = "healthy", service = "DSAGrind.Gateway.API", timestamp = DateTime.UtcNow });
app.MapGet("/", () => "DSAGrind API Gateway - Routing traffic to microservices");

try
{
    Log.Information("Starting DSAGrind Gateway API on port 5000");
    app.Run("http://0.0.0.0:5000");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}