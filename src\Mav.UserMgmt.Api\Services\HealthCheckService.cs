using Mav.UserMgmt.Api.Models.HealthCheck;
using System.Diagnostics;
using System.Reflection;

namespace Mav.UserMgmt.Api.Services;

/// <summary>
/// Provides health check business logic, evaluating critical dependencies
/// and returning an aggregated health status result.
/// </summary>
public sealed class HealthCheckService : IHealthCheckService
{
    private static readonly string Version =
        Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        ?? "unknown";

    /// <inheritdoc />
    public async Task<HealthCheckResponse> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var dependencies = new Dictionary<string, DependencyHealthStatus>();
        var overallHealthy = true;

        // Check each dependency and collect results
        var selfCheck = await CheckSelfAsync(cancellationToken);
        dependencies["self"] = selfCheck;

        if (selfCheck.Status != HealthStatus.Healthy)
        {
            overallHealthy = false;
        }

        var overallStatus = overallHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy;

        return new HealthCheckResponse
        {
            Status = overallStatus,
            Timestamp = DateTimeOffset.UtcNow,
            Version = Version,
            Dependencies = dependencies
        };
    }

    private static Task<DependencyHealthStatus> CheckSelfAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // Basic self-check: application is running
        var status = new DependencyHealthStatus
        {
            Status = HealthStatus.Healthy,
            Description = "Application is running",
            DurationMs = stopwatch.ElapsedMilliseconds
        };

        stopwatch.Stop();
        status.DurationMs = stopwatch.ElapsedMilliseconds;

        return Task.FromResult(status);
    }
}
