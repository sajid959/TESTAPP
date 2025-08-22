using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.AI.API.Controllers;

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
            service = "DSAGrind.AI.API",
            timestamp = DateTime.UtcNow 
        });
    }
}