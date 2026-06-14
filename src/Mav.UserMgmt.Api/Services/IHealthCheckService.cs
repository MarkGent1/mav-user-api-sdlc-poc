using Mav.UserMgmt.Api.Models;

namespace Mav.UserMgmt.Api.Services;

public interface IHealthCheckService
{
    Task<HealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);
}
