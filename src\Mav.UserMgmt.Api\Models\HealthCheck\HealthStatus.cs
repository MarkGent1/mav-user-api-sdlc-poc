namespace Mav.UserMgmt.Api.Models.HealthCheck;

/// <summary>
/// Defines string constants for health status values.
/// </summary>
public static class HealthStatus
{
    /// <summary>The service is healthy.</summary>
    public const string Healthy = "healthy";

    /// <summary>The service is degraded but still operational.</summary>
    public const string Degraded = "degraded";

    /// <summary>The service is unhealthy.</summary>
    public const string Unhealthy = "unhealthy";
}
