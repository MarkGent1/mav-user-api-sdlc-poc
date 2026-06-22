using Mav.UserMgmt.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mav.UserMgmt.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthCheckController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;

    public HealthCheckController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var response = _healthCheckService.GetHealthStatus();
        return Ok(response);
    }
}
