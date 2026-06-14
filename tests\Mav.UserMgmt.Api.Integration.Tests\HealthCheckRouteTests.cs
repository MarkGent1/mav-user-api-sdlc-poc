using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Mav.UserMgmt.Api.Models.HealthCheck;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Mav.UserMgmt.Api.Integration.Tests;

/// <summary>
/// Integration tests that spin up the real application and make HTTP requests
/// to the /api/health endpoint.
/// </summary>
public class HealthCheckRouteTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthCheckRouteTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsStatusCode200()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealth_ReturnsContentTypeApplicationJson()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task GetHealth_ResponseBody_ContainsStatusField()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("status", out _),
            "Response body should contain a 'status' field.");
    }

    [Fact]
    public async Task GetHealth_ResponseBody_StatusIsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString();
        Assert.Equal(HealthStatus.Healthy, status);
    }

    [Fact]
    public async Task GetHealth_ResponseBody_ContainsTimestampField()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("timestamp", out _),
            "Response body should contain a 'timestamp' field.");
    }

    [Fact]
    public async Task GetHealth_ResponseBody_ContainsVersionField()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("version", out var versionElement),
            "Response body should contain a 'version' field.");
        var version = versionElement.GetString();
        Assert.NotNull(version);
        Assert.NotEmpty(version);
    }

    [Fact]
    public async Task GetHealth_ResponseBody_ContainsDependenciesField()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("dependencies", out _),
            "Response body should contain a 'dependencies' field.");
    }

    [Fact]
    public async Task GetHealth_ResponseBody_DependenciesContainsSelf()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        using var doc = JsonDocument.Parse(body);
        var dependencies = doc.RootElement.GetProperty("dependencies");
        Assert.True(dependencies.TryGetProperty("self", out _),
            "Dependencies should contain a 'self' entry.");
    }

    [Fact]
    public async Task GetHealth_CanDeserializeFullResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        var healthCheckResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.NotNull(healthCheckResponse);
        Assert.Equal(HealthStatus.Healthy, healthCheckResponse.Status);
        Assert.NotNull(healthCheckResponse.Version);
        Assert.NotEmpty(healthCheckResponse.Version);
        Assert.NotNull(healthCheckResponse.Dependencies);
        Assert.True(healthCheckResponse.Timestamp > DateTimeOffset.MinValue);
    }

    [Fact]
    public async Task GetHealth_RouteIsAccessibleWithoutAuthentication()
    {
        // The endpoint should be anonymous — no auth headers needed.
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert — must not return 401 or 403
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetHealth_RouteIsRegistered_NotNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
