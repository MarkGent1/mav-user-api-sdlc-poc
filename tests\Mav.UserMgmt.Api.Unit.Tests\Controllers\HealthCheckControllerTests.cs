using Mav.UserMgmt.Api.Controllers;
using Mav.UserMgmt.Api.Models.HealthCheck;
using Mav.UserMgmt.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mav.UserMgmt.Api.Unit.Tests.Controllers;

public class HealthCheckControllerTests
{
    private readonly FakeHealthCheckService _fakeService;
    private readonly HealthCheckController _sut;

    public HealthCheckControllerTests()
    {
        _fakeService = new FakeHealthCheckService();
        _sut = new HealthCheckController(_fakeService);
    }

    [Fact]
    public async Task GetHealthAsync_ReturnsOk_WhenHealthy()
    {
        // Arrange
        _fakeService.Response = new HealthCheckResponse
        {
            Status = HealthStatus.Healthy,
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0",
            Dependencies = new Dictionary<string, DependencyHealthStatus>
            {
                ["self"] = new DependencyHealthStatus { Status = HealthStatus.Healthy, Description = "OK", DurationMs = 0 }
            }
        };

        // Act
        var result = await _sut.GetHealthAsync();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetHealthAsync_Returns503_WhenUnhealthy()
    {
        // Arrange
        _fakeService.Response = new HealthCheckResponse
        {
            Status = HealthStatus.Unhealthy,
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0",
            Dependencies = new Dictionary<string, DependencyHealthStatus>
            {
                ["self"] = new DependencyHealthStatus { Status = HealthStatus.Unhealthy, Description = "Down", DurationMs = 0 }
            }
        };

        // Act
        var result = await _sut.GetHealthAsync();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetHealthAsync_ReturnsOk_WhenDegraded()
    {
        // Arrange
        _fakeService.Response = new HealthCheckResponse
        {
            Status = HealthStatus.Degraded,
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0",
            Dependencies = new Dictionary<string, DependencyHealthStatus>()
        };

        // Act
        var result = await _sut.GetHealthAsync();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetHealthAsync_ReturnsHealthCheckResponse_AsPayload()
    {
        // Arrange
        var expected = new HealthCheckResponse
        {
            Status = HealthStatus.Healthy,
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0",
            Dependencies = new Dictionary<string, DependencyHealthStatus>()
        };
        _fakeService.Response = expected;

        // Act
        var result = await _sut.GetHealthAsync();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Same(expected, objectResult.Value);
    }

    [Fact]
    public async Task GetHealthAsync_PassesCancellationToken_ToService()
    {
        // Arrange
        _fakeService.Response = new HealthCheckResponse
        {
            Status = HealthStatus.Healthy,
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0",
            Dependencies = new Dictionary<string, DependencyHealthStatus>()
        };
        using var cts = new CancellationTokenSource();

        // Act
        await _sut.GetHealthAsync(cts.Token);

        // Assert
        Assert.Equal(cts.Token, _fakeService.ReceivedCancellationToken);
    }

    private sealed class FakeHealthCheckService : IHealthCheckService
    {
        public HealthCheckResponse Response { get; set; } = new HealthCheckResponse
        {
            Status = HealthStatus.Healthy,
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0",
            Dependencies = new Dictionary<string, DependencyHealthStatus>()
        };

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<HealthCheckResponse> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(Response);
        }
    }
}
