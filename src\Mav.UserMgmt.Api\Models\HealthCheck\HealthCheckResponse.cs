namespace Mav.UserMgmt.Api.Models.HealthCheck;

/// <summary>
/// Represents the health check response payload returned by the health check endpoint.
/// </summary>
public sealed class HealthCheckResponse
{
    /// <summary>
    /// Gets or sets the overall health status of the service.
    /// Expected values: 'healthy', 'degraded', 'unhealthy'.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp at which the health check was performed.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the version of the running application.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a dictionary of individual dependency/component health statuses.
    /// Key: dependency name. Value: <see cref="DependencyHealthStatus"/>.
    /// </summary>
    public Dictionary<string, DependencyHealthStatus> Dependencies { get; set; } = new();
}
