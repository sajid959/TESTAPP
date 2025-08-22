using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/", () => "🎉 DSAGrind .NET Gateway API is running!");
app.MapGet("/health", () => new { 
    status = "healthy", 
    service = "DSAGrind.Gateway.API", 
    timestamp = DateTime.UtcNow,
    message = "✅ Migration to .NET microservices complete!"
});

app.Run("http://0.0.0.0:5000");