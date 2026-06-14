namespace Mav.UserMgmt.Api.Models;

/// <summary>
/// Represents the overall health status of the API.
/// </summary>
public class HealthStatus
{
    /// <summary>
    /// Overall health status: "UP" or "DOWN".
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp of when the health check was performed.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The application version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// A map of component-level health details (e.g., database, cache, api).
    /// </summary>
    public Dictionary<string, ComponentHealthStatus> Components { get; set; } = new();
}

/// <summary>
/// Represents the health status of an individual component.
/// </summary>
public class ComponentHealthStatus
{
    /// <summary>
    /// Component health status: "UP" or "DOWN".
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Optional description providing additional context.
    /// </summary>
    public string? Description { get; set; }
}
