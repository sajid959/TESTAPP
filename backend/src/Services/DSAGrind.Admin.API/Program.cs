using DSAGrind.Admin.API.Services;
using DSAGrind.Common.Extensions;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;

// Load environment variables from .env files before creating builder
EnvironmentExtensions.LoadEnvFile();

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/admin-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Configure your token validation parameters here
        };
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<DSAGrind.Common.Configuration.JwtSettings>();
        options.Authority = builder.Configuration.GetValue<string>("Auth:Authority") ?? "http://localhost:8080";
        options.Audience = jwtSettings?.Audience ?? "DSAGrind-Users";
        options.RequireHttpsMetadata = false; // <-- Add this line for development
    });
builder.Services.AddCommonServices(builder.Configuration);
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IContentModerationService, ContentModerationService>();

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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DSAGrind Admin API v1");
        c.RoutePrefix = "swagger"; // Explicitly set the route prefix to /swagger
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => new { status = "healthy", service = "DSAGrind.Admin.API", timestamp = DateTime.UtcNow });

try
{
    Log.Information("Starting DSAGrind Admin API on port 5005");
    app.Run("http://localhost:5005");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Admin API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}