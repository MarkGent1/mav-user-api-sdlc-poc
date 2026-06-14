# ADR 004: Health Check Endpoint Design

## Status

Accepted

## Context

The User Management API requires a health check endpoint so that:

- Load balancers can determine whether to route traffic to an instance.
- Kubernetes liveness and readiness probes can detect unhealthy pods.
- Monitoring and alerting systems can observe service health.
- CI/CD pipelines can verify a deployment is healthy before completing a rollout.

## Decision

### Endpoint

- **Route:** `GET /api/health`
- **Authentication:** None — the endpoint is anonymous and does not require an auth token.
- **Controller:** `HealthCheckController` in `Mav.UserMgmt.Api.Controllers`
- **Service:** `IHealthCheckService` / `HealthCheckService` in `Mav.UserMgmt.Api.Services`

### Response Design

The endpoint returns a JSON body (`HealthCheckResponse`) containing:

| Field          | Description                                              |
|----------------|----------------------------------------------------------|
| `status`       | Aggregated status: `healthy`, `degraded`, or `unhealthy` |
| `timestamp`    | UTC time the check was performed                         |
| `version`      | Assembly informational version                           |
| `dependencies` | Dictionary of individual dependency health statuses      |

### HTTP Status Codes

| Status      | HTTP Code |
|-------------|-----------|
| `healthy`   | 200       |
| `degraded`  | 200       |
| `unhealthy` | 503       |

Returning `200` for `degraded` allows load balancers to continue routing traffic while the degraded condition is being investigated.

### Router Registration

Controllers are mapped with `.AllowAnonymous()` applied globally at the endpoint mapping level in `WebApplicationExtensions.ConfigureRequestPipeline()`:

```csharp
app.MapControllers().AllowAnonymous();
```

This ensures the health endpoint does not require authentication without adding `[AllowAnonymous]` to each controller individually.

### Infrastructure

Load balancers, Docker health checks, and Kubernetes probes should all use `/api/health` with an expected HTTP `200` response as the success criterion.

## Consequences

- The health endpoint is always reachable without credentials, which is the standard pattern for infrastructure health probes.
- Additional dependency checks (database, external services) can be added to `HealthCheckService` without changing the controller or API contract.
- The `version` field allows operators to confirm which build is deployed at a glance.
