using Mav.UserMgmt.Api.Controllers;
using Mav.UserMgmt.Api.Models;
using Mav.UserMgmt.Api.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Mav.UserMgmt.Api.Unit.Tests.Controllers;

public class HealthCheckControllerTests
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly HealthCheckController _sut;

    public HealthCheckControllerTests()
    {
        _healthCheckService = Substitute.For<IHealthCheckService>();
        _sut = new HealthCheckController(_healthCheckService);
    }

    [Fact]
    public async Task Get_ReturnsOkResult_WhenHealthStatusIsUp()
    {
        // Arrange
        var healthStatus = new HealthStatus
        {
            Status = "UP",
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, ComponentHealthStatus>
            {
                ["api"] = new ComponentHealthStatus { Status = "UP", Description = "API is running" },
                ["database"] = new ComponentHealthStatus { Status = "UP", Description = "Database connection is healthy" }
            }
        };
        _healthCheckService.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(healthStatus);

        // Act
        var result = await _sut.Get(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedStatus = Assert.IsType<HealthStatus>(okResult.Value);
        Assert.Equal("UP", returnedStatus.Status);
    }

    [Fact]
    public async Task Get_Returns503_WhenHealthStatusIsDown()
    {
        // Arrange
        var healthStatus = new HealthStatus
        {
            Status = "DOWN",
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, ComponentHealthStatus>
            {
                ["api"] = new ComponentHealthStatus { Status = "UP", Description = "API is running" },
                ["database"] = new ComponentHealthStatus { Status = "DOWN", Description = "Database connection failed" }
            }
        };
        _healthCheckService.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(healthStatus);

        // Act
        var result = await _sut.Get(CancellationToken.None);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, statusCodeResult.StatusCode);
        var returnedStatus = Assert.IsType<HealthStatus>(statusCodeResult.Value);
        Assert.Equal("DOWN", returnedStatus.Status);
    }

    [Fact]
    public async Task Get_ReturnsHealthStatusWithComponents_WhenHealthy()
    {
        // Arrange
        var healthStatus = new HealthStatus
        {
            Status = "UP",
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, ComponentHealthStatus>
            {
                ["api"] = new ComponentHealthStatus { Status = "UP", Description = "API is running" },
                ["database"] = new ComponentHealthStatus { Status = "UP", Description = "Database connection is healthy" }
            }
        };
        _healthCheckService.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(healthStatus);

        // Act
        var result = await _sut.Get(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStatus = Assert.IsType<HealthStatus>(okResult.Value);
        Assert.NotNull(returnedStatus.Components);
        Assert.True(returnedStatus.Components.ContainsKey("api"));
        Assert.True(returnedStatus.Components.ContainsKey("database"));
        Assert.Equal("UP", returnedStatus.Components["api"].Status);
        Assert.Equal("UP", returnedStatus.Components["database"].Status);
    }

    [Fact]
    public async Task Get_ReturnsHealthStatusWithDownComponents_WhenUnhealthy()
    {
        // Arrange
        var healthStatus = new HealthStatus
        {
            Status = "DOWN",
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, ComponentHealthStatus>
            {
                ["api"] = new ComponentHealthStatus { Status = "UP", Description = "API is running" },
                ["database"] = new ComponentHealthStatus { Status = "DOWN", Description = "Database connection failed" }
            }
        };
        _healthCheckService.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(healthStatus);

        // Act
        var result = await _sut.Get(CancellationToken.None);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, statusCodeResult.StatusCode);
        var returnedStatus = Assert.IsType<HealthStatus>(statusCodeResult.Value);
        Assert.Equal("DOWN", returnedStatus.Components["database"].Status);
        Assert.Equal("UP", returnedStatus.Components["api"].Status);
    }

    [Fact]
    public async Task Get_ReturnsHealthStatusWithTimestamp_WhenHealthy()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var healthStatus = new HealthStatus
        {
            Status = "UP",
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, ComponentHealthStatus>
            {
                ["api"] = new ComponentHealthStatus { Status = "UP" },
                ["database"] = new ComponentHealthStatus { Status = "UP" }
            }
        };
        _healthCheckService.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(healthStatus);

        // Act
        var result = await _sut.Get(CancellationToken.None);
        var after = DateTime.UtcNow;

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStatus = Assert.IsType<HealthStatus>(okResult.Value);
        Assert.True(returnedStatus.Timestamp >= before.AddSeconds(-1));
        Assert.True(returnedStatus.Timestamp <= after.AddSeconds(1));
    }

    [Fact]
    public async Task Get_CallsHealthCheckService_WithCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var healthStatus = new HealthStatus
        {
            Status = "UP",
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, ComponentHealthStatus>()
        };
        _healthCheckService.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(healthStatus);

        // Act
        await _sut.Get(cancellationToken);

        // Assert
        await _healthCheckService.Received(1).CheckHealthAsync(cancellationToken);
    }

    [Fact]
    public async Task Get_ThrowsException_WhenHealthCheckServiceThrows()
    {
        // Arrange
        _healthCheckService.CheckHealthAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service initialization error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Get(CancellationToken.None));
    }

    [Fact]
    public async Task Get_ReturnsOkResult_WithUpStatusBody_WhenHealthy()
    {
        // Arrange
        var healthStatus = new HealthStatus
        {
            Status = "UP",
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, ComponentHealthStatus>()
        };
        _healthCheckService.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(healthStatus);

        // Act
        var result = await _sut.Get(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
        var returnedStatus = okResult.Value as HealthStatus;
        Assert.NotNull(returnedStatus);
        Assert.Equal("UP", returnedStatus.Status);
    }

    [Fact]
    public async Task Get_Returns503Result_WithDownStatusBody_WhenUnhealthy()
    {
        // Arrange
        var healthStatus = new HealthStatus
        {
            Status = "DOWN",
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, ComponentHealthStatus>()
        };
        _healthCheckService.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(healthStatus);

        // Act
        var result = await _sut.Get(CancellationToken.None);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
        var returnedStatus = statusCodeResult.Value as HealthStatus;
        Assert.NotNull(returnedStatus);
        Assert.Equal("DOWN", returnedStatus.Status);
    }
}
