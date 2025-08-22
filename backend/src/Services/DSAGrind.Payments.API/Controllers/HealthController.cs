using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Payments.API.Controllers;

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
            service = "DSAGrind.Payments.API",
            timestamp = DateTime.UtcNow 
        });
    }
}