using Mav.UserMgmt.Api.Models.HealthCheck;
using Mav.UserMgmt.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mav.UserMgmt.Api.Controllers;

/// <summary>
/// Exposes the health check endpoint for the API.
/// </summary>
[ApiController]
[Route("api/health")]
public sealed class HealthCheckController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;

    /// <summary>
    /// Initializes a new instance of <see cref="HealthCheckController"/>.
    /// </summary>
    /// <param name="healthCheckService">The health check service.</param>
    public HealthCheckController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Returns the health status of the API and its dependencies.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// HTTP 200 with a <see cref="HealthCheckResponse"/> payload when healthy or degraded;
    /// HTTP 503 with the same payload when unhealthy.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        var response = await _healthCheckService.CheckHealthAsync(cancellationToken);

        var statusCode = response.Status == HealthStatus.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;

        return StatusCode(statusCode, response);
    }
}
