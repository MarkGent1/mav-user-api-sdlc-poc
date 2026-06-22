using System.Net;
using System.Net.Http.Json;
using Mav.UserMgmt.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Mav.UserMgmt.Api.Integration.Tests;

public class HealthCheckEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthCheckEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOkStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealth_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Contains("application/json", response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(healthResponse);
        Assert.Equal("Healthy", healthResponse.Status);
    }

    [Fact]
    public async Task GetHealth_ReturnsNonNullTimestamp()
    {
        // Act
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(healthResponse);
        Assert.NotEqual(default(DateTime), healthResponse.Timestamp);
    }

    [Fact]
    public async Task GetHealth_ReturnsNonEmptyEnvironment()
    {
        // Act
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(healthResponse);
        Assert.False(string.IsNullOrEmpty(healthResponse.Environment));
    }

    [Fact]
    public async Task GetHealth_ReturnsNonEmptyVersion()
    {
        // Act
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(healthResponse);
        Assert.False(string.IsNullOrEmpty(healthResponse.Version));
    }

    [Fact]
    public async Task GetHealth_ReturnsCompleteHealthCheckResponse()
    {
        // Act
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var healthResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(healthResponse);
        Assert.Equal("Healthy", healthResponse.Status);
        Assert.NotEqual(default(DateTime), healthResponse.Timestamp);
        Assert.False(string.IsNullOrEmpty(healthResponse.Environment));
        Assert.False(string.IsNullOrEmpty(healthResponse.Version));
    }
}
