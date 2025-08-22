using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly HttpClient _httpClient;

    public HealthController(ILogger<HealthController> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    [HttpGet]
    public async Task<ActionResult> GetHealth()
    {
        var services = new Dictionary<string, object>
        {
            { "gateway", "healthy" },
            { "timestamp", DateTime.UtcNow }
        };

        var serviceChecks = new Dictionary<string, string>
        {
            { "auth", "http://localhost:8080/api/health" },
            { "problems", "http://localhost:5001/api/health" },
            { "submissions", "http://localhost:5002/api/health" },
            { "ai", "http://localhost:5003/api/health" },
            { "search", "http://localhost:5004/api/health" },
            { "admin", "http://localhost:5005/api/health" },
            { "payments", "http://localhost:5006/api/health" }
        };

        foreach (var service in serviceChecks)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var response = await _httpClient.GetAsync(service.Value, cts.Token);
                services[service.Key] = response.IsSuccessStatusCode ? "healthy" : "unhealthy";
            }
            catch
            {
                services[service.Key] = "unavailable";
            }
        }

        var allHealthy = services.Values.OfType<string>().All(status => status == "healthy");
        
        if (allHealthy)
        {
            return Ok(new { status = "healthy", services });
        }
        
        return StatusCode(503, new { status = "degraded", services });
    }

    [HttpGet("ready")]
    public ActionResult GetReadiness()
    {
        return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
    }
}