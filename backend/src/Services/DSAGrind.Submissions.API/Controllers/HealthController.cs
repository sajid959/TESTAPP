using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Submissions.API.Controllers;

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
            service = "DSAGrind.Submissions.API",
            timestamp = DateTime.UtcNow 
        });
    }
}