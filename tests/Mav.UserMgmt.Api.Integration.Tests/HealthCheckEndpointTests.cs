using System.Net;
using System.Text.Json;
using Mav.UserMgmt.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Mav.UserMgmt.Api.Integration.Tests;

public class HealthCheckEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private WebApplicationFactory<Program> CreateFactoryWithDatabaseChecker(bool isConnected)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing IDatabaseConnectionChecker registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDatabaseConnectionChecker));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var mockChecker = Substitute.For<IDatabaseConnectionChecker>();
                mockChecker.IsConnectedAsync(Arg.Any<CancellationToken>()).Returns(isConnected);
                services.AddScoped<IDatabaseConnectionChecker>(_ => mockChecker);
            });
        });
    }

    [Fact]
    public async Task Get_Health_ReturnsOk_WhenAllDependenciesHealthy()
    {
        // Arrange
        var factory = CreateFactoryWithDatabaseChecker(true);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Health_ReturnsServiceUnavailable_WhenDatabaseUnhealthy()
    {
        // Arrange
        var factory = CreateFactoryWithDatabaseChecker(false);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Get_Health_ReturnsJsonContentType_WhenAllHealthy()
    {
        // Arrange
        var factory = CreateFactoryWithDatabaseChecker(true);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Contains("application/json", response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task Get_Health_ReturnsUpStatus_WhenAllDependenciesHealthy()
    {
        // Arrange
        var factory = CreateFactoryWithDatabaseChecker(true);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("UP", doc.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Get_Health_ReturnsDownStatus_WhenDatabaseUnhealthy()
    {
        // Arrange
        var factory = CreateFactoryWithDatabaseChecker(false);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("DOWN", doc.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Get_Health_ReturnsResponseWithTimestamp_WhenAllHealthy()
    {
        // Arrange
        var factory = CreateFactoryWithDatabaseChecker(true);
        var client = factory.CreateClient();
        var before = DateTime.UtcNow;

        // Act
        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var timestamp = doc.RootElement.GetProperty("timestamp").GetDateTime();
        Assert.True(timestamp >= before.AddSeconds(-1));
        Assert.True(timestamp <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task Get_Health_ReturnsResponseWithComponents_WhenAllHealthy()
    {
        // Arrange
        var factory = CreateFactoryWithDatabaseChecker(true);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var components = doc.RootElement.GetProperty("components");
        Assert.True(components.TryGetProperty("api", out var apiComponent));
        Assert.Equal("UP", apiComponent.GetProperty("status").GetString());
        Assert.True(components.TryGetProperty("database", out var dbComponent));
        Assert.Equal("UP", dbComponent.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Get_Health_ReturnsDatabaseDownInComponents_WhenDatabaseUnhealthy()
    {
        // Arrange
        var factory = CreateFactoryWithDatabaseChecker(false);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var components = doc.RootElement.GetProperty("components");
        Assert.True(components.TryGetProperty("database", out var dbComponent));
        Assert.Equal("DOWN", dbComponent.GetProperty("status").GetString());
        Assert.True(components.TryGetProperty("api", out var apiComponent));
        Assert.Equal("UP", apiComponent.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Get_Health_ReturnsServiceUnavailable_WhenDatabaseThrows()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDatabaseConnectionChecker));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var mockChecker = Substitute.For<IDatabaseConnectionChecker>();
                mockChecker.IsConnectedAsync(Arg.Any<CancellationToken>())
                    .Returns<bool>(_ => throw new InvalidOperationException("Connection error"));
                services.AddScoped<IDatabaseConnectionChecker>(_ => mockChecker);
            });
        });
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Get_Health_IsAccessibleWithoutAuthentication()
    {
        // Arrange - use a client that doesn't follow redirects, so we can detect auth redirects
        var factory = CreateFactoryWithDatabaseChecker(true);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/health");

        // Assert - should not return 401 Unauthorized or 302 redirect to login
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Health_RouteIsRegisteredAndAccessible()
    {
        // Arrange
        var factory = CreateFactoryWithDatabaseChecker(true);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert - route exists (not 404)
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Health_ResponseBodyIsValidJson()
    {
        // Arrange
        var factory = CreateFactoryWithDatabaseChecker(true);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body));
        var exception = Record.Exception(() => JsonDocument.Parse(body));
        Assert.Null(exception);
    }
}
