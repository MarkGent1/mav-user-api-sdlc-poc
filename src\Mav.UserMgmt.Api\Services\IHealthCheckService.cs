using Mav.UserMgmt.Api.Models.HealthCheck;

namespace Mav.UserMgmt.Api.Services;

/// <summary>
/// Defines the contract for the health check service.
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Evaluates the health of the application and its dependencies.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="HealthCheckResponse"/> containing the aggregated health status.</returns>
    Task<HealthCheckResponse> CheckHealthAsync(CancellationToken cancellationToken = default);
}
