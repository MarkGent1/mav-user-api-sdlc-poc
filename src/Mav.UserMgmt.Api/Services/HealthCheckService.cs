using Mav.UserMgmt.Api.Interfaces;
using Mav.UserMgmt.Api.Models;

namespace Mav.UserMgmt.Api.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public HealthCheckService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public HealthCheckResponse GetHealthStatus()
    {
        return new HealthCheckResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            ApplicationName = _environment.ApplicationName,
            EnvironmentName = _environment.EnvironmentName
        };
    }
}
