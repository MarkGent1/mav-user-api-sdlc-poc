namespace Mav.UserMgmt.Api.Models.HealthCheck;

/// <summary>
/// Represents the health status of an individual service dependency.
/// </summary>
public sealed class DependencyHealthStatus
{
    /// <summary>
    /// Gets or sets the health status of the dependency.
    /// Expected values: 'healthy', 'degraded', 'unhealthy'.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description or message providing additional context
    /// about the dependency's health status.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the duration in milliseconds taken to check the dependency.
    /// </summary>
    public long? DurationMs { get; set; }
}
