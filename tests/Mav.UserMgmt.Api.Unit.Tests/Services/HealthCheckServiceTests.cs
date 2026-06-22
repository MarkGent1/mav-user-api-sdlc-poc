using Mav.UserMgmt.Api.Models;
using Mav.UserMgmt.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Mav.UserMgmt.Api.Unit.Tests.Services;

public class HealthCheckServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly HealthCheckService _service;

    public HealthCheckServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Test");
        _service = new HealthCheckService(_mockConfiguration.Object, _mockEnvironment.Object);
    }

    [Fact]
    public void GetHealthStatus_ReturnsHealthyStatus()
    {
        // Act
        var result = _service.GetHealthStatus();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Healthy", result.Status);
    }

    [Fact]
    public void GetHealthStatus_ReturnsCorrectEnvironmentName()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
        var service = new HealthCheckService(_mockConfiguration.Object, _mockEnvironment.Object);

        // Act
        var result = service.GetHealthStatus();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Production", result.Environment);
    }

    [Fact]
    public void GetHealthStatus_ReturnsDevelopmentEnvironmentName()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
        var service = new HealthCheckService(_mockConfiguration.Object, _mockEnvironment.Object);

        // Act
        var result = service.GetHealthStatus();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Development", result.Environment);
    }

    [Fact]
    public void GetHealthStatus_ReturnsNonDefaultTimestamp()
    {
        // Act
        var result = _service.GetHealthStatus();

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(default(DateTime), result.Timestamp);
    }

    [Fact]
    public void GetHealthStatus_ReturnsUtcTimestamp()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var result = _service.GetHealthStatus();

        // Assert
        var after = DateTime.UtcNow.AddSeconds(1);
        Assert.NotNull(result);
        Assert.True(result.Timestamp >= before && result.Timestamp <= after);
    }

    [Fact]
    public void GetHealthStatus_ReturnsNonEmptyVersion()
    {
        // Act
        var result = _service.GetHealthStatus();

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Version));
    }

    [Fact]
    public void GetHealthStatus_ReturnsVersionWithExpectedFormat()
    {
        // Act
        var result = _service.GetHealthStatus();

        // Assert
        Assert.NotNull(result);
        // Version should be in major.minor.build.revision format or fallback
        Assert.Matches(@"^\d+\.\d+\.\d+\.\d+$", result.Version);
    }

    [Fact]
    public void GetHealthStatus_ReturnsCompleteHealthCheckResponse()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Staging");
        var service = new HealthCheckService(_mockConfiguration.Object, _mockEnvironment.Object);

        // Act
        var result = service.GetHealthStatus();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Healthy", result.Status);
        Assert.Equal("Staging", result.Environment);
        Assert.NotEqual(default(DateTime), result.Timestamp);
        Assert.False(string.IsNullOrEmpty(result.Version));
    }

    [Fact]
    public void GetHealthStatus_ReturnsHealthCheckResponseType()
    {
        // Act
        var result = _service.GetHealthStatus();

        // Assert
        Assert.IsType<HealthCheckResponse>(result);
    }

    [Fact]
    public void GetHealthStatus_CalledMultipleTimes_ReturnsHealthyEachTime()
    {
        // Act
        var result1 = _service.GetHealthStatus();
        var result2 = _service.GetHealthStatus();
        var result3 = _service.GetHealthStatus();

        // Assert
        Assert.Equal("Healthy", result1.Status);
        Assert.Equal("Healthy", result2.Status);
        Assert.Equal("Healthy", result3.Status);
    }

    [Fact]
    public void GetHealthStatus_WithDifferentEnvironments_ReturnsCorrectEnvironmentName()
    {
        // Arrange
        var environments = new[] { "Development", "Staging", "Production", "Test", "QA" };

        foreach (var environmentName in environments)
        {
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns(environmentName);
            var service = new HealthCheckService(_mockConfiguration.Object, _mockEnvironment.Object);

            // Act
            var result = service.GetHealthStatus();

            // Assert
            Assert.Equal(environmentName, result.Environment);
        }
    }

    [Fact]
    public void GetHealthStatus_UsesEnvironmentFromIWebHostEnvironment()
    {
        // Arrange
        var expectedEnvironment = "CustomEnvironment";
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(expectedEnvironment);
        var service = new HealthCheckService(_mockConfiguration.Object, _mockEnvironment.Object);

        // Act
        var result = service.GetHealthStatus();

        // Assert
        _mockEnvironment.Verify(e => e.EnvironmentName, Times.AtLeastOnce);
        Assert.Equal(expectedEnvironment, result.Environment);
    }

    [Fact]
    public void GetHealthStatus_StatusIsNotUnhealthy()
    {
        // Act
        var result = _service.GetHealthStatus();

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual("Unhealthy", result.Status);
        Assert.NotEqual("Degraded", result.Status);
    }
}
