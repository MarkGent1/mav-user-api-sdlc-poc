namespace Mav.UserMgmt.Api.Models;

public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = string.Empty;
}
