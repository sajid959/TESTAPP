using DSAGrind.Common.Extensions;
using DSAGrind.AI.API.Services;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ai-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Common Services
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

// Add Auto Mapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Add Services
builder.Services.AddScoped<IAIService, AIService>();

// Add CORS
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

// Configure the HTTP request pipeline
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

// Health check
app.MapGet("/health", () => new { status = "healthy", service = "DSAGrind.AI.API", timestamp = DateTime.UtcNow });

try
{
    var port = builder.Configuration.GetValue<string>("AI:Port") ?? "5003";
    var host = builder.Configuration.GetValue<string>("AI:Host") ?? "0.0.0.0";
    var url = $"http://{host}:{port}";
    
    Log.Information($"Starting DSAGrind AI API on {url}");
    app.Run(url);
}
catch (Exception ex)
{
    Log.Fatal(ex, "AI API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}