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
        var version = typeof(HealthCheckService).Assembly
            .GetName()
            .Version?
            .ToString() ?? "1.0.0.0";

        return new HealthCheckResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Environment = _environment.EnvironmentName,
            Version = version
        };
    }
}
