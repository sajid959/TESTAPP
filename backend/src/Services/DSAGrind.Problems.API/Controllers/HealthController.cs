using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Problems.API.Controllers;

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
            service = "DSAGrind.Problems.API",
            timestamp = DateTime.UtcNow 
        });
    }
}