using Mav.UserMgmt.Api.Services;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var healthStatus = await _healthCheckService.CheckHealthAsync(cancellationToken);

        if (healthStatus.Status == "UP")
        {
            return Ok(healthStatus);
        }

        return StatusCode(503, healthStatus);
    }
}
