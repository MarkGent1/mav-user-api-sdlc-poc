using Mav.UserMgmt.Api.Models.HealthCheck;
using Mav.UserMgmt.Api.Services;

namespace Mav.UserMgmt.Api.Unit.Tests.Services;

public class HealthCheckServiceTests
{
    private readonly IHealthCheckService _sut;

    public HealthCheckServiceTests()
    {
        _sut = new HealthCheckService();
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsHealthCheckResponse()
    {
        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsHealthyStatus_WhenAllDependenciesAreHealthy()
    {
        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsTimestamp()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.True(result.Timestamp >= before);
        Assert.True(result.Timestamp <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsVersion()
    {
        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.NotNull(result.Version);
        Assert.NotEmpty(result.Version);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsDependencies()
    {
        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.NotNull(result.Dependencies);
        Assert.NotEmpty(result.Dependencies);
    }

    [Fact]
    public async Task CheckHealthAsync_IncludesSelfDependency()
    {
        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.True(result.Dependencies.ContainsKey("self"));
    }

    [Fact]
    public async Task CheckHealthAsync_SelfDependencyIsHealthy()
    {
        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Dependencies["self"].Status);
    }

    [Fact]
    public async Task CheckHealthAsync_SelfDependencyHasDurationMs()
    {
        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.NotNull(result.Dependencies["self"].DurationMs);
        Assert.True(result.Dependencies["self"].DurationMs >= 0);
    }

    [Fact]
    public async Task CheckHealthAsync_SelfDependencyHasDescription()
    {
        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.NotNull(result.Dependencies["self"].Description);
        Assert.NotEmpty(result.Dependencies["self"].Description);
    }

    [Fact]
    public async Task CheckHealthAsync_RespectsCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _sut.CheckHealthAsync(cts.Token);

        // Assert
        Assert.NotNull(result);
    }
}
