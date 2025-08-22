using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Admin.API.Controllers;

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
            service = "DSAGrind.Admin.API",
            timestamp = DateTime.UtcNow 
        });
    }
}