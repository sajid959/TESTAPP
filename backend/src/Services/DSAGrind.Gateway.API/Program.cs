using DSAGrind.Common.Extensions;
using Serilog;

// Load environment variables from .env files before creating builder
DSAGrind.Common.Extensions.EnvironmentExtensions.LoadEnvFile();

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

// Add API documentation  
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "DSAGrind Gateway API", Version = "v1" });
});

// Add reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});
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

// Configure development-only services
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DSAGrind Gateway API v1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
}

// Rate limiter disabled for now
//app.UseCors("AllowAll");
app.UseCors("AllowFrontend");

// Map controllers first
app.MapControllers();

// Map reverse proxy
app.MapReverseProxy();

app.MapGet("/health", () => new { status = "healthy", service = "DSAGrind.Gateway.API", timestamp = DateTime.UtcNow });
app.MapGet("/", () => "DSAGrind API Gateway - Routing traffic to microservices");

try
{
    var port = builder.Configuration.GetValue<string>("Gateway:Port") ?? "5000";
    var host = builder.Configuration.GetValue<string>("Gateway:Host") ?? "localhost";
    var url = $"http://{host}:{port}";
    
    Log.Information($"Starting DSAGrind Gateway API on {url}");
    app.Run(url);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}