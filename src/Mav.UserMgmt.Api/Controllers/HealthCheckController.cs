using Microsoft.AspNetCore.Mvc;

namespace Mav.UserMgmt.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow
        });
    }
}
