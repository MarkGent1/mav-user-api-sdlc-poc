using Mav.UserMgmt.Api.Data;
using Mav.UserMgmt.Api.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Mav.UserMgmt.Api.Unit.Tests.Services;

public class HealthCheckServiceTests
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IDatabaseConnectionChecker _databaseConnectionChecker;
    private readonly HealthCheckService _sut;

    public HealthCheckServiceTests()
    {
        _logger = Substitute.For<ILogger<HealthCheckService>>();
        _databaseConnectionChecker = Substitute.For<IDatabaseConnectionChecker>();
        _databaseConnectionChecker.IsConnectedAsync(Arg.Any<CancellationToken>()).Returns(true);
        _sut = new HealthCheckService(_logger, _databaseConnectionChecker);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsUpStatus_WhenAllComponentsHealthy()
    {
        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UP", result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsTimestamp_WithCurrentUtcTime()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        var after = DateTime.UtcNow;
        Assert.True(result.Timestamp >= before && result.Timestamp <= after);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsComponents_WithApiComponent()
    {
        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.NotNull(result.Components);
        Assert.True(result.Components.ContainsKey("api"));
        Assert.Equal("UP", result.Components["api"].Status);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsComponents_WithDatabaseComponent()
    {
        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.NotNull(result.Components);
        Assert.True(result.Components.ContainsKey("database"));
        Assert.Equal("UP", result.Components["database"].Status);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsDownStatus_WhenDatabaseUnhealthy()
    {
        // Arrange
        _databaseConnectionChecker.IsConnectedAsync(Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.Equal("DOWN", result.Status);
        Assert.Equal("DOWN", result.Components["database"].Status);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsDatabaseDownStatus_WhenConnectionCheckerThrows()
    {
        // Arrange
        _databaseConnectionChecker.IsConnectedAsync(Arg.Any<CancellationToken>())
            .Returns<bool>(x => throw new InvalidOperationException("Connection error"));

        // Act
        var result = await _sut.CheckHealthAsync();

        // Assert
        Assert.Equal("DOWN", result.Status);
        Assert.Equal("DOWN", result.Components["database"].Status);
    }
}
