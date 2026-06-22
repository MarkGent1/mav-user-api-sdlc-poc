using Mav.UserMgmt.Api.Controllers;
using Mav.UserMgmt.Api.Interfaces;
using Mav.UserMgmt.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Mav.UserMgmt.Api.Unit.Tests.Controllers;

public class HealthCheckControllerTests
{
    private readonly Mock<IHealthCheckService> _mockHealthCheckService;
    private readonly HealthCheckController _controller;

    public HealthCheckControllerTests()
    {
        _mockHealthCheckService = new Mock<IHealthCheckService>();
        _controller = new HealthCheckController(_mockHealthCheckService.Object);
    }

    [Fact]
    public void Get_ReturnsOkResult()
    {
        // Arrange
        var healthCheckResponse = new HealthCheckResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Environment = "Test",
            Version = "1.0.0.0"
        };

        _mockHealthCheckService
            .Setup(s => s.GetHealthStatus())
            .Returns(healthCheckResponse);

        // Act
        var result = _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public void Get_ReturnsHealthyStatus()
    {
        // Arrange
        var healthCheckResponse = new HealthCheckResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Environment = "Test",
            Version = "1.0.0.0"
        };

        _mockHealthCheckService
            .Setup(s => s.GetHealthStatus())
            .Returns(healthCheckResponse);

        // Act
        var result = _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HealthCheckResponse>(okResult.Value);
        Assert.Equal("Healthy", response.Status);
    }

    [Fact]
    public void Get_ReturnsResponseFromService()
    {
        // Arrange
        var expectedResponse = new HealthCheckResponse
        {
            Status = "Healthy",
            Timestamp = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Environment = "Production",
            Version = "2.0.0.0"
        };

        _mockHealthCheckService
            .Setup(s => s.GetHealthStatus())
            .Returns(expectedResponse);

        // Act
        var result = _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HealthCheckResponse>(okResult.Value);
        Assert.Equal(expectedResponse.Status, response.Status);
        Assert.Equal(expectedResponse.Timestamp, response.Timestamp);
        Assert.Equal(expectedResponse.Environment, response.Environment);
        Assert.Equal(expectedResponse.Version, response.Version);
    }

    [Fact]
    public void Get_CallsHealthCheckServiceOnce()
    {
        // Arrange
        var healthCheckResponse = new HealthCheckResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Environment = "Test",
            Version = "1.0.0.0"
        };

        _mockHealthCheckService
            .Setup(s => s.GetHealthStatus())
            .Returns(healthCheckResponse);

        // Act
        _controller.Get();

        // Assert
        _mockHealthCheckService.Verify(s => s.GetHealthStatus(), Times.Once);
    }

    [Fact]
    public void Get_ReturnsNonNullResponse()
    {
        // Arrange
        var healthCheckResponse = new HealthCheckResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Environment = "Test",
            Version = "1.0.0.0"
        };

        _mockHealthCheckService
            .Setup(s => s.GetHealthStatus())
            .Returns(healthCheckResponse);

        // Act
        var result = _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}
