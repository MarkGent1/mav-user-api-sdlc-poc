# Health Check Endpoint

## Overview

The User Management API exposes a health check endpoint that can be used by load balancers, orchestration platforms (e.g., Kubernetes), and monitoring systems to verify that the service is running and healthy.

## Endpoint

| Property        | Value                          |
|-----------------|--------------------------------|
| **Method**      | `GET`                          |
| **Path**        | `/api/health`                  |
| **Auth**        | None (anonymous, no token required) |
| **Content-Type**| `application/json`             |

## Response

### HTTP 200 OK — Healthy or Degraded

Returned when the service is operational (status: `healthy` or `degraded`).

```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T10:30:00.000Z",
  "version": "1.0.0+abc1234",
  "dependencies": {
    "self": {
      "status": "healthy",
      "description": "Application is running",
      "durationMs": 0
    }
  }
}
```

### HTTP 503 Service Unavailable — Unhealthy

Returned when the service is unhealthy (status: `unhealthy`). The response body has the same schema.

```json
{
  "status": "unhealthy",
  "timestamp": "2024-01-15T10:30:00.000Z",
  "version": "1.0.0+abc1234",
  "dependencies": {
    "self": {
      "status": "unhealthy",
      "description": "Application check failed",
      "durationMs": 5
    }
  }
}
```

## Response Schema

### `HealthCheckResponse`

| Field          | Type                                      | Description                                              |
|----------------|-------------------------------------------|----------------------------------------------------------|
| `status`       | `string`                                  | Overall health status: `healthy`, `degraded`, `unhealthy` |
| `timestamp`    | `string` (ISO 8601 UTC)                   | UTC timestamp when the health check was performed        |
| `version`      | `string`                                  | Informational version of the running application         |
| `dependencies` | `object<string, DependencyHealthStatus>`  | Map of dependency name to individual health status       |

### `DependencyHealthStatus`

| Field         | Type      | Description                                               |
|---------------|-----------|-----------------------------------------------------------|
| `status`      | `string`  | Health status of the dependency: `healthy`, `degraded`, `unhealthy` |
| `description` | `string?` | Optional message providing context about the dependency   |
| `durationMs`  | `number?` | Duration in milliseconds taken to check the dependency    |

## Status Values

| Value       | HTTP Status | Meaning                                        |
|-------------|-------------|------------------------------------------------|
| `healthy`   | `200`       | All checks pass; service is fully operational  |
| `degraded`  | `200`       | Service is operational but with reduced capability |
| `unhealthy` | `503`       | One or more critical checks have failed        |

## Infrastructure Configuration

### Docker Compose

The health check path is `/api/health`. When configuring a `HEALTHCHECK` instruction in a Dockerfile or a `healthcheck` block in `docker-compose.yml`, use:

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/api/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 10s
```

### Kubernetes Liveness / Readiness Probes

```yaml
livenessProbe:
  httpGet:
    path: /api/health
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 30
  timeoutSeconds: 5
  failureThreshold: 3

readinessProbe:
  httpGet:
    path: /api/health
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3
```

### Load Balancer Health Check Path

Configure your load balancer (e.g., AWS ALB, Azure Application Gateway, NGINX) to use:

- **Health check path:** `/api/health`
- **Expected HTTP status:** `200`
- **Protocol:** HTTP or HTTPS

## Router Registration

The endpoint is registered via the ASP.NET Core MVC controller pipeline in `src/Mav.UserMgmt.Api/Setup/WebApplicationExtensions.cs`:

```csharp
app.MapControllers().AllowAnonymous();
```

The controller is decorated with `[Route("api/health")]` and `[ApiController]`, and the action `GetHealthAsync` is decorated with `[HttpGet]`. No authentication is required — the endpoint is explicitly accessible anonymously.

## OpenAPI / Swagger

When running in the `Development` environment, the OpenAPI document is available at `/openapi/v1.json` (via `app.MapOpenApi()`). The `HealthCheckController` is included automatically because `AddControllers()` is called during service registration.

The action is annotated with:

```csharp
[ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status503ServiceUnavailable)]
```

This ensures the OpenAPI document accurately reflects both possible response codes and the response body schema.
