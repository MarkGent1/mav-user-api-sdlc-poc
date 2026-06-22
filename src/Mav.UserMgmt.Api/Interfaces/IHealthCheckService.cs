using Mav.UserMgmt.Api.Models;

namespace Mav.UserMgmt.Api.Interfaces;

public interface IHealthCheckService
{
    HealthCheckResponse GetHealthStatus();
}
