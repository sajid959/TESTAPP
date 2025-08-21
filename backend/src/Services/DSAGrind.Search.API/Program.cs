using DSAGrind.Common.Extensions;
using DSAGrind.Search.API.Services;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/search-service-.txt", rollingInterval: RollingInterval.Day)
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
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IVectorSearchService, VectorSearchService>();

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
app.MapGet("/health", () => new { status = "healthy", service = "DSAGrind.Search.API", timestamp = DateTime.UtcNow });

try
{
    Log.Information("Starting DSAGrind Search API on port 5004");
    app.Run("http://0.0.0.0:5004");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Search API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}