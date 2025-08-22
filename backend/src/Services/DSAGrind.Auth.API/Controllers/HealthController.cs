using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult GetHealth()
    {
        return Ok(new 
        { 
            status = "healthy", 
            service = "DSAGrind.Auth.API",
            timestamp = DateTime.UtcNow 
        });
    }
}