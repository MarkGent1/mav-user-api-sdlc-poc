using Mav.UserMgmt.Api.Data;
using Mav.UserMgmt.Api.Models;

namespace Mav.UserMgmt.Api.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IDatabaseConnectionChecker _databaseConnectionChecker;

    public HealthCheckService(ILogger<HealthCheckService> logger, IDatabaseConnectionChecker databaseConnectionChecker)
    {
        _logger = logger;
        _databaseConnectionChecker = databaseConnectionChecker;
    }

    public async Task<HealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var components = new Dictionary<string, ComponentHealthStatus>
        {
            ["api"] = new ComponentHealthStatus
            {
                Status = "UP",
                Description = "API is running"
            }
        };

        components["database"] = await CheckDatabaseHealthAsync(cancellationToken);

        var overallHealthy = components.Values.All(c => c.Status == "UP");

        return new HealthStatus
        {
            Status = overallHealthy ? "UP" : "DOWN",
            Timestamp = DateTime.UtcNow,
            Components = components
        };
    }

    private async Task<ComponentHealthStatus> CheckDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            var isConnected = await _databaseConnectionChecker.IsConnectedAsync(cancellationToken);
            return new ComponentHealthStatus
            {
                Status = isConnected ? "UP" : "DOWN",
                Description = isConnected ? "Database connection is healthy" : "Database connection failed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return new ComponentHealthStatus
            {
                Status = "DOWN",
                Description = ex.Message
            };
        }
    }
}
