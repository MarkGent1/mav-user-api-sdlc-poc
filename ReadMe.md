# Mav User Management API

A .NET 10 Web API for user management, built following Clean Architecture principles.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/) (optional, for container-based development)

### Running Locally

```bash
cd src/Mav.UserMgmt.Api
dotnet run
```

The API will start on the default ASP.NET Core ports (`http://localhost:5000` / `https://localhost:5001`).

### Running with Docker Compose

```bash
docker-compose up
```

## API Endpoints

### Health Check

| Method | Path          | Auth     | Description                          |
|--------|---------------|----------|--------------------------------------|
| `GET`  | `/api/health` | None     | Returns the health status of the API |

#### Example Response (200 OK)

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

See [docs/api/health-check.md](docs/api/health-check.md) for full API documentation including response schemas and infrastructure configuration.

### Root

| Method | Path | Auth | Description        |
|--------|------|------|--------------------|
| `GET`  | `/`  | None | Liveness ping      |

## Infrastructure

### Load Balancer / Kubernetes Health Check

Configure your load balancer or Kubernetes probes to use:

- **Path:** `/api/health`
- **Expected HTTP status:** `200`

See [docs/api/health-check.md](docs/api/health-check.md) for full infrastructure configuration examples.

## OpenAPI / Swagger

When running in the `Development` environment, the OpenAPI document is available at:

```
GET /openapi/v1.json
```

## Project Structure

```
src/
  Mav.UserMgmt.Api/          # Main API project
    Controllers/             # API controllers
    Models/                  # Request/response models
    Services/                # Business logic services
    Setup/                   # DI and middleware configuration
tests/
  Mav.UserMgmt.Api.Unit.Tests/        # Unit tests
  Mav.UserMgmt.Api.Integration.Tests/ # Integration tests
  Mav.UserMgmt.Api.Component.Tests/   # Component tests
docs/
  api/                       # API documentation
  adr/                       # Architecture Decision Records
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/Mav.UserMgmt.Api.Unit.Tests

# Run integration tests only
dotnet test tests/Mav.UserMgmt.Api.Integration.Tests
```
