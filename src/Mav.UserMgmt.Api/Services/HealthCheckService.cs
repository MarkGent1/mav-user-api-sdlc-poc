using Mav.UserMgmt.Api.Interfaces;
using Mav.UserMgmt.Api.Models;

namespace Mav.UserMgmt.Api.Services;

public class HealthCheckService : IHealthCheckService
{
    private const string HealthyStatus = "Healthy";
    private const string DefaultVersion = "1.0.0.0";

    private readonly IWebHostEnvironment _environment;

    public HealthCheckService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public HealthCheckResponse GetHealthStatus()
    {
        var version = typeof(HealthCheckService).Assembly
            .GetName()
            .Version?
            .ToString() ?? DefaultVersion;

        return new HealthCheckResponse
        {
            Status = HealthyStatus,
            Timestamp = DateTime.UtcNow,
            Environment = _environment.EnvironmentName,
            Version = version
        };
    }
}
