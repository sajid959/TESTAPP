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
app.MapGet("/health", () => new { status = "healthy", service = "DSAGrind.Problems.API", timestamp = DateTime.UtcNow });

try
{
    Log.Information("Starting DSAGrind Problems API on port 5001");
    app.Run("http://0.0.0.0:5001");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Problems API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}