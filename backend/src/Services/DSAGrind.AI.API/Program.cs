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

// Add Auto Mapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Add Services
builder.Services.AddScoped<IAIService, AIService>();

// Add CORS
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
    Log.Information("Starting DSAGrind AI API on port 5003");
    app.Run("http://0.0.0.0:5003");
}
catch (Exception ex)
{
    Log.Fatal(ex, "AI API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}