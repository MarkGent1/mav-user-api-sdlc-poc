using Mav.UserMgmt.Api.Controllers;
using Mav.UserMgmt.Api.Models.HealthCheck;
using Mav.UserMgmt.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

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

    [Fact]
    public void GetHealthAsync_DoesNotRequireAuthentication()
    {
        // The endpoint is configured via AllowAnonymous() on app.MapControllers() in the pipeline.
        // At the controller level, we verify no [Authorize] attribute is present on the controller or action.
        var controllerType = typeof(HealthCheckController);
        var authorizeOnController = controllerType.GetCustomAttributes(inherit: true)
            .Any(a => a.GetType().Name == "AuthorizeAttribute");

        var getHealthMethod = controllerType.GetMethod(nameof(HealthCheckController.GetHealthAsync));
        var authorizeOnMethod = getHealthMethod!.GetCustomAttributes(inherit: true)
            .Any(a => a.GetType().Name == "AuthorizeAttribute");

        Assert.False(authorizeOnController, "HealthCheckController should not have [Authorize] attribute.");
        Assert.False(authorizeOnMethod, "GetHealthAsync should not have [Authorize] attribute.");
    }

    [Fact]
    public void HealthCheckController_HasHttpGetAttribute_OnGetHealthAsync()
    {
        // Verify the action is decorated with [HttpGet]
        var controllerType = typeof(HealthCheckController);
        var getHealthMethod = controllerType.GetMethod(nameof(HealthCheckController.GetHealthAsync));
        var hasHttpGet = getHealthMethod!.GetCustomAttributes(inherit: true)
            .Any(a => a.GetType().Name == "HttpGetAttribute");

        Assert.True(hasHttpGet, "GetHealthAsync should have [HttpGet] attribute.");
    }

    [Fact]
    public void HealthCheckController_HasApiControllerAttribute()
    {
        // Verify the controller has [ApiController]
        var controllerType = typeof(HealthCheckController);
        var hasApiController = controllerType.GetCustomAttributes(inherit: true)
            .Any(a => a.GetType().Name == "ApiControllerAttribute");

        Assert.True(hasApiController, "HealthCheckController should have [ApiController] attribute.");
    }

    [Fact]
    public void HealthCheckController_HasCorrectRoute()
    {
        // Verify the controller route is 'api/health'
        var controllerType = typeof(HealthCheckController);
        var routeAttribute = controllerType.GetCustomAttributes(inherit: true)
            .FirstOrDefault(a => a.GetType().Name == "RouteAttribute");

        Assert.NotNull(routeAttribute);
        var templateProperty = routeAttribute.GetType().GetProperty("Template");
        var template = templateProperty?.GetValue(routeAttribute) as string;
        Assert.Equal("api/health", template);
    }

    [Fact]
    public async Task GetHealthAsync_ResponseBody_HasStatusField_WhenHealthy()
    {
        // Arrange
        _fakeService.Response = new HealthCheckResponse
        {
            Status = HealthStatus.Healthy,
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0",
            Dependencies = new Dictionary<string, DependencyHealthStatus>()
        };

        // Act
        var result = await _sut.GetHealthAsync();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<HealthCheckResponse>(objectResult.Value);
        Assert.Equal(HealthStatus.Healthy, response.Status);
    }

    [Fact]
    public async Task GetHealthAsync_ResponseBody_HasTimestamp()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;
        _fakeService.Response = new HealthCheckResponse
        {
            Status = HealthStatus.Healthy,
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0",
            Dependencies = new Dictionary<string, DependencyHealthStatus>()
        };

        // Act
        var result = await _sut.GetHealthAsync();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<HealthCheckResponse>(objectResult.Value);
        Assert.True(response.Timestamp >= before);
    }

    [Fact]
    public async Task GetHealthAsync_ResponseBody_HasVersion()
    {
        // Arrange
        _fakeService.Response = new HealthCheckResponse
        {
            Status = HealthStatus.Healthy,
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0",
            Dependencies = new Dictionary<string, DependencyHealthStatus>()
        };

        // Act
        var result = await _sut.GetHealthAsync();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<HealthCheckResponse>(objectResult.Value);
        Assert.NotNull(response.Version);
        Assert.NotEmpty(response.Version);
    }

    [Fact]
    public async Task GetHealthAsync_ResponseBody_HasDependencies()
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
        var response = Assert.IsType<HealthCheckResponse>(objectResult.Value);
        Assert.NotNull(response.Dependencies);
    }

    [Fact]
    public async Task GetHealthAsync_Returns200_WithStatusOk_AsString()
    {
        // Arrange - verify the string value matches expected 'healthy' constant
        _fakeService.Response = new HealthCheckResponse
        {
            Status = HealthStatus.Healthy,
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0",
            Dependencies = new Dictionary<string, DependencyHealthStatus>()
        };

        // Act
        var result = await _sut.GetHealthAsync();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        var response = Assert.IsType<HealthCheckResponse>(objectResult.Value);
        Assert.Equal("healthy", response.Status);
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
