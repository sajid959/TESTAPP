using DSAGrind.Common.Extensions;
using DSAGrind.Problems.API.Repositories;
using DSAGrind.Problems.API.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/problems-service-.txt", rollingInterval: RollingInterval.Day)
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

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Add Repositories
builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Add Services
builder.Services.AddScoped<IProblemService, ProblemService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

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
app.MapGet("/health", () => new { status = "healthy", service = "DSAGrind.Problems.API", timestamp = DateTime.UtcNow });

try
{
    var port = builder.Configuration.GetValue<string>("Problems:Port") ?? "5001";
    var host = builder.Configuration.GetValue<string>("Problems:Host") ?? "0.0.0.0";
    var url = $"http://{host}:{port}";
    
    Log.Information($"Starting DSAGrind Problems API on {url}");
    app.Run(url);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Problems API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}