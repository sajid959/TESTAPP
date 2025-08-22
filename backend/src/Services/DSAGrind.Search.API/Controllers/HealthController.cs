using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Search.API.Controllers;

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
            service = "DSAGrind.Search.API",
            timestamp = DateTime.UtcNow 
        });
    }
}