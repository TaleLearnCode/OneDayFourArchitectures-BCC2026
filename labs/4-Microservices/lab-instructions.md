# Module 4 Lab: Microservices — Complete the Alert Service

## Introduction: What Are Microservices, and Why Do They Matter?

After Module 1 (Monolith), Module 2 (N-Tier), and Module 3 (Modular Monolith), you've seen three ways to structure one deployable application. Module 4 changes the deployment boundary: each business capability now runs as its own process.

**Cipher's Grid** is a distributed race operations platform. Instead of one application containing every feature, the platform is split into independently deployable services:

- **API Gateway** — one entry point for clients
- **Crew Service** — driver and crew data
- **Race Service** — race scheduling and race entries
- **Telemetry Service** — race telemetry
- **Alert Service** — operational alerts and acknowledgments

This architecture gives teams stronger independence than a modular monolith. Each service owns its own API, database, container, deployment unit, and failure boundary. The cost is real: services now communicate over HTTP, run in multiple containers, and can fail independently.

**This lab is designed to make you experience the microservices tradeoff directly:** you will complete one service, wire its storage and API, run the platform with Docker Compose, and verify another service can call it over the network.

---

## What You're Building

The scaffold already includes a multi-service solution with Docker Compose orchestration:

| Service | Responsibility | Starter state |
|---|---|---|
| `CiphersGrid.Api` | API Gateway / reverse proxy | Complete |
| `CiphersGrid.CrewService` | Driver and crew records | Complete |
| `CiphersGrid.RaceService` | Race creation and entries | Complete; already calls Alert Service |
| `CiphersGrid.TelemetryService` | Telemetry endpoints | Complete |
| `CiphersGrid.AlertService` | Race alerts and acknowledgments | Stubbed — you will complete it |
| `CiphersGrid.SharedKernel` | Shared IDs, enums, and simple service DTOs | Complete |

The missing piece is the **Alert Service**. Race Service already attempts to notify Alert Service when a driver is added to a race, but Alert Service currently has only a health stub. Your job is to complete Alert Service so it can receive, persist, list, filter, and acknowledge alerts.

By the end of this lab, you will have:

- Implemented the Alert Service domain model, DTOs, repository, service, controller, and startup wiring
- Added a SQLite database owned by Alert Service
- Created an EF Core migration for the Alert Service schema
- Ran all services together with Docker Compose
- Verified Race Service triggers Alert Service through service-to-service HTTP
- Verified alerts can be retrieved and acknowledged through Swagger

That's the microservices experience: one feature crosses process boundaries, not just project boundaries.

---

## Getting Started

**Project location:** `labs/4-Microservices/Lab-start/`

Before you begin, open **Docker Desktop** and wait until it reports that the Docker engine is running. You won't use Docker until Step 10, but starting it now avoids a common delay later.

Open the solution:

- `Sections/4-Microservices/lab-start/CiphersGrid.sln`

From a terminal in the `Lab-start` folder, restore dependencies:

```powershell
Set-Location labs\4-Microservices\lab-start
dotnet restore .\CiphersGrid.sln
```

### Step 0 — Verify the Starter State

Before completing Alert Service, verify the platform scaffold is present.

**You should see these projects in the solution:**

- `CiphersGrid.Api`
- `CiphersGrid.CrewService`
- `CiphersGrid.RaceService`
- `CiphersGrid.TelemetryService`
- `CiphersGrid.AlertService`
- `CiphersGrid.SharedKernel`
- `CiphersGrid.AlertService.Tests.xUnit`

**Build the solution:**

```powershell
dotnet build .\CiphersGrid.sln
```

The solution should build before you start. The Alert Service is stubbed, but the project exists and compiles.

**Starter behavior to notice:**

- `RaceService` already has an `AlertServiceClient`
- `docker-compose.yml` already defines an `alert-service` container
- `AlertService` currently has placeholder files and a minimal `/health` endpoint.
- Race Service gracefully ignores Alert Service failures, which is a common microservices resilience pattern.

> **If the solution does not build:** Run `dotnet restore` and verify Docker Desktop/.NET SDK prerequisites are installed. Make sure you opened the `Lab-start` solution, not `Lab-end`.
>
> **Docker reminder:** If Docker Desktop is not already running, start it now and let the engine finish loading before you reach Step 10.

---

## Step 1 — Understand the Service Boundary

### What You're Doing

Before writing Alert Service code, take a minute to understand what changes when a module becomes a microservice.

In Module 3, Penalties and Results lived in one process. A service could call another service through an interface in memory. In Module 4, Race Service and Alert Service are separate processes. Race Service calls Alert Service through HTTP.

### Step 1a: Inspect the Existing Race → Alert Call

**File:** `src\CiphersGrid.RaceService\Services\RaceService.cs`

Find the `AddRaceEntryAsync` method. After Race Service saves a race entry, it calls Alert Service:

```csharp
await alertServiceClient.CreateAlertAsync(new(
    raceId,
    driverId,
    "Broadcast",
    "Low",
    $"Driver {driverId} registered for race {raceId}"
));
```

**Key points:**

- Race Service does not reference Alert Service classes.
- It sends an HTTP request through `AlertServiceClient`.
- The call is wrapped in `try/catch` so Race Service can still succeed if Alert Service is unavailable.

That is the microservices tradeoff: service independence improves isolation, but every cross-service call can fail.

### Step 1b: Inspect Service Discovery in Docker Compose

**File:** `docker-compose.yml`

Find the Race Service environment variable:

```yaml
- ServiceUrls__AlertService=http://alert-service:8080
```

Inside Docker Compose, `alert-service` is the DNS name for the Alert Service container. Race Service does not call `localhost`; it calls the container name.

**Key points:**

- `localhost` from inside a container means that same container, not your machine.
- Docker Compose creates a network where services resolve each other by service name.
- This is why the client uses `http://alert-service:8080`.

### Step 1c: Inspect the Alert Service Stub

**Files:**

- `src\CiphersGrid.AlertService\Program.cs`
- `src\CiphersGrid.AlertService\Models\Alert.cs`
- `src\CiphersGrid.AlertService\Data\AlertDbContext.cs`
- `src\CiphersGrid.AlertService\Repositories\AlertRepository.cs`
- `src\CiphersGrid.AlertService\Services\AlertService.cs`
- `src\CiphersGrid.AlertService\Controllers\AlertsController.cs`

These files exist, but several are placeholders. You will replace the stubs with a complete, working Alert Service.

---

## Step 2 — Implement the Alert Domain Model

### What You're Doing

The domain model is the data Alert Service owns. In a microservices architecture, each service owns its own model and database schema. Other services do not query this table directly.

**File:** `src\CiphersGrid.AlertService\Models\Alert.cs`

Replace the entire file with:

```csharp
using CiphersGrid.SharedKernel.Enums;

namespace CiphersGrid.AlertService.Models;

public class Alert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid RaceId { get; set; }
    public required Guid DriverId { get; set; }
    public required AlertType AlertType { get; set; }
    public required AlertSeverity Severity { get; set; }
    public required string Message { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public bool IsAcknowledged { get; set; } = false;
}
```

**Key points:**

- `RaceId` and `DriverId` are copied into the alert because Alert Service owns its own persistence.
- `AlertType` and `AlertSeverity` come from `SharedKernel,` so services agree on common vocabulary.
- `IsAcknowledged` lets Alert Service manage the alert lifecycle independently.

---

## Step 3 — Implement the Alert Database Context

### What You're Doing

Alert Service needs its own database context. This is not a shared platform database. It is Alert Service's local persistence boundary.

**File:** `src\CiphersGrid.AlertService\Data\AlertDbContext.cs`

Replace the entire file with:

```csharp
using CiphersGrid.AlertService.Models;
using Microsoft.EntityFrameworkCore;

namespace CiphersGrid.AlertService.Data;

public class AlertDbContext(DbContextOptions<AlertDbContext> options) : DbContext(options)
{
    public DbSet<Alert> Alerts { get; set; }
}
```

**Key points:**

- `AlertDbContext` belongs only to the Alert Service.
- No other service should inject or query this context.
- The `Alerts` `DbSet` is what EF Core uses to create the `Alerts` table.

---

## Step 4 — Define the Alert API Contracts

### What You're Doing

The HTTP API is the public contract for Alert Service. Race Service and API clients should use DTOs, not EF Core entities.

**File:** `src\CiphersGrid.AlertService\DTOs\AlertDtos.cs`

Replace the entire file with:

```csharp
namespace CiphersGrid.AlertService.DTOs;

public record CreateAlertRequest(Guid RaceId, Guid DriverId, string AlertType, string Severity, string Message);
public record AlertResponseDto(Guid Id, Guid RaceId, Guid DriverId, string AlertType, string Severity, string Message, DateTime IssuedAt, bool IsAcknowledged);
```

**Key points:**

- `CreateAlertRequest` is the POST body Race Service sends.
- `AlertResponseDto` is what Alert Service returns to callers.
- Keeping DTOs separate from the entity lets the service change its database model later without breaking clients.

---

## Step 5 — Implement Alert Data Access

### What You're Doing

The repository is the service's data access boundary. It keeps EF Core queries out of the controller and service orchestration code.

**File:** `src\CiphersGrid.AlertService\Repositories\AlertRepository.cs`

Replace the entire file with:

```csharp
using CiphersGrid.AlertService.Models;
using CiphersGrid.AlertService.Data;

namespace CiphersGrid.AlertService.Repositories;

public class AlertRepository(AlertDbContext context)
{
    public async Task<Alert?> GetByIdAsync(Guid id)
    {
        return await context.Alerts.FindAsync(id);
    }

    public async Task<IEnumerable<Alert>> GetByRaceIdAsync(Guid raceId)
    {
        return context.Alerts.Where(a => a.RaceId == raceId).OrderByDescending(a => a.IssuedAt).ToList();
    }

    public async Task<IEnumerable<Alert>> GetAllAsync()
    {
        return context.Alerts.OrderByDescending(a => a.IssuedAt).ToList();
    }

    public async Task<Alert> AddAsync(Alert alert)
    {
        await context.Alerts.AddAsync(alert);
        await context.SaveChangesAsync();
        return alert;
    }

    public async Task UpdateAsync(Alert alert)
    {
        context.Alerts.Update(alert);
        await context.SaveChangesAsync();
    }

    public async Task AcknowledgeAsync(Guid alertId)
    {
        var alert = await GetByIdAsync(alertId);
        if (alert != null)
        {
            alert.IsAcknowledged = true;
            await UpdateAsync(alert);
        }
    }
}
```

**Key points:**

- `GetByRaceIdAsync` supports filtering alerts generated for one race.
- `AddAsync` saves a new alert when Race Service calls Alert Service.
- `UpdateAsync` persists acknowledgment changes.

---

## Step 6 — Implement Alert Business Logic

### What You're Doing

The service layer converts HTTP-friendly strings into domain enums, creates alert entities, and maps entities back to DTOs.

**File:** `src\CiphersGrid.AlertService\Services\AlertService.cs`

Replace the entire file with:

```csharp
using CiphersGrid.AlertService.DTOs;
using CiphersGrid.AlertService.Models;
using CiphersGrid.AlertService.Repositories;
using CiphersGrid.SharedKernel.Enums;

namespace CiphersGrid.AlertService.Services;

public class AlertService(AlertRepository alertRepository)
{
    public async Task<AlertResponseDto> CreateAlertAsync(
        Guid raceId,
        Guid driverId,
        string alertType,
        string severity,
        string message)
    {
        if (!Enum.TryParse<AlertType>(alertType, true, out var type))
            type = AlertType.Broadcast;
        
        if (!Enum.TryParse<AlertSeverity>(severity, true, out var sev))
            sev = AlertSeverity.Low;

        var alert = new Alert
        {
            RaceId = raceId,
            DriverId = driverId,
            AlertType = type,
            Severity = sev,
            Message = message
        };

        var created = await alertRepository.AddAsync(alert);
        return MapToDto(created);
    }

    public async Task<IEnumerable<AlertResponseDto>> GetAlertsForRaceAsync(Guid raceId)
    {
        var alerts = await alertRepository.GetByRaceIdAsync(raceId);
        return alerts.Select(MapToDto);
    }

    public async Task<IEnumerable<AlertResponseDto>> GetAllAlertsAsync()
    {
        var alerts = await alertRepository.GetAllAsync();
        return alerts.Select(MapToDto);
    }

    public async Task<AlertResponseDto?> AcknowledgeAlertAsync(Guid alertId)
    {
        var alert = await alertRepository.GetByIdAsync(alertId);
        if (alert is null) return null;

        alert.IsAcknowledged = true;
        await alertRepository.UpdateAsync(alert);
        return MapToDto(alert);
    }

    private static AlertResponseDto MapToDto(Alert alert)
    {
        return new AlertResponseDto(
            alert.Id,
            alert.RaceId,
            alert.DriverId,
            alert.AlertType.ToString(),
            alert.Severity.ToString(),
            alert.Message,
            alert.IssuedAt,
            alert.IsAcknowledged
        );
    }
}
```

**Key points:**

- `Enum.TryParse` keeps Alert Service tolerant of simple string payloads.
- `CreateAlertAsync` is the method Race Service indirectly exercises through HTTP.
- `AcknowledgeAlertAsync` owns the state transition from unacknowledged to acknowledged.

---

## Step 7 — Expose Alert HTTP Endpoints

### What You're Doing

The controller exposes Alert Service over HTTP. This is the contract other services and clients use.

**File:** `src\CiphersGrid.AlertService\Controllers\AlertsController.cs`

Replace the entire file with:

```csharp
using Microsoft.AspNetCore.Mvc;
using CiphersGrid.AlertService.DTOs;
using AlertServiceClass = CiphersGrid.AlertService.Services.AlertService;

namespace CiphersGrid.AlertService.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController(AlertServiceClass alertService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAlert([FromBody] CreateAlertRequest request)
    {
        var alert = await alertService.CreateAlertAsync(
            request.RaceId,
            request.DriverId,
            request.AlertType,
            request.Severity,
            request.Message);
        
        return CreatedAtAction(nameof(GetAlert), new { id = alert.Id }, alert);
    }

    [HttpGet]
    public async Task<IActionResult> GetAlerts([FromQuery] Guid? raceId)
    {
        if (raceId.HasValue)
        {
            var alerts = await alertService.GetAlertsForRaceAsync(raceId.Value);
            return Ok(alerts);
        }
        
        var allAlerts = await alertService.GetAllAlertsAsync();
        return Ok(allAlerts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAlert(Guid id)
    {
        // Not implemented in this version, but could return single alert
        return Ok(new { id });
    }

    [HttpPut("{id}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlert(Guid id)
    {
        var alert = await alertService.AcknowledgeAlertAsync(id);
        if (alert is null) return NotFound();
        
        return Ok(alert);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok("Alert Service is healthy");
    }
}
```

**Endpoints added:**

| Endpoint | Purpose |
|---|---|
| `POST /api/alerts` | Create an alert |
| `GET /api/alerts` | List all alerts |
| `GET /api/alerts?raceId={raceId}` | List alerts for one race |
| `PUT /api/alerts/{id}/acknowledge` | Mark an alert acknowledged |
| `GET /api/alerts/health` | Service-specific health check |

**Key points:**

- Race Service calls `POST /api/alerts`.
- The gateway can route `/api/alerts` traffic to Alert Service.
- Alert Service's API is independent from Race Service's API.

---

## Step 8 — Wire Alert Service Startup

### What You're Doing

`Program.cs` is the composition root for Alert Service. It registers the database, repository, service, controllers, Swagger, health endpoint, and startup migration behavior.

**File:** `src\CiphersGrid.AlertService\Program.cs`

Replace the entire file with:

```csharp
using CiphersGrid.AlertService.Data;
using CiphersGrid.AlertService.Repositories;
using CiphersGrid.AlertService.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AlertDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AlertDb")));

builder.Services.AddScoped<AlertRepository>();
builder.Services.AddScoped<AlertService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Alert Service API",
        Version = "v1",
        Description = "Module 4: Microservices - Crew intelligence alerts"
    });
});

WebApplication app = builder.Build();

// Apply migrations at startup
using (IServiceScope scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AlertDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Alert Service v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => "OK")
    .WithName("Health")
    .Produces(200);

app.Run();
```

**Key points:**

- `AddDbContext` uses Alert Service's own connection string.
- `MigrateAsync()` applies Alert Service migrations when the container starts.
- Swagger runs inside the Alert Service container at `http://localhost:5300/swagger`.

---

## Step 9 — Create the Alert Service Migration

### What You're Doing

`AlertDbContext` defines the model, but EF Core still needs a migration file to create the SQLite table. The migration belongs to Alert Service because Alert Service owns the Alerts database.

From `labs\4-Microservices\lab-start`, run:

```powershell
# Run this only if "dotnet ef" is not recognized:
# dotnet tool install --global dotnet-ef --version 10.*

dotnet ef migrations add InitialCreate --project .\src\CiphersGrid.AlertService --startup-project .\src\CiphersGrid.AlertService
```

**Expected result:** new files under `src\CiphersGrid.AlertService\Migrations\`:

- `*_InitialCreate.cs` (timestamp prefix can vary)
- `AlertDbContextModelSnapshot.cs`

> **If EF reports that the startup project needs `Microsoft.EntityFrameworkCore.Design`:** add the package to Alert Service using the same EF Core version as the other EF packages, then rerun the migration command:
>
> ```powershell
> dotnet add .\src\CiphersGrid.AlertService\CiphersGrid.AlertService.csproj package Microsoft.EntityFrameworkCore.Design --version 10.0.11
> ```

---

## Step 10 — Run the Platform with Docker Compose

### What You're Doing

Now run the distributed system. Docker Compose builds each service into a container, creates a shared network, and starts the gateway plus all backend services.

Before you run Compose, make sure **Docker Desktop is open and the Docker engine is running**. On Windows, the lab uses Docker Desktop's Linux engine; if it is stopped, Docker commands fail before they can build any service images.

From `labs\4-Microservices\lab-start`, run:

```powershell
docker compose up --build -d
```

**Expected services:**

- `api-gateway`
- `crew-service`
- `race-service`
- `telemetry-service`
- `alert-service`

Docker Compose may prefix the actual container names with the folder/project name, such as `lab-start-race-service-1`. The service names are what matter.

**Expected URLs:**

| Service | URL |
|---|---|
| Gateway Swagger | `http://localhost:5000/swagger` (gateway-owned endpoints only) |
| Crew Service Swagger | `http://localhost:5100/swagger` |
| Race Service Swagger | `http://localhost:5200/swagger` |
| Telemetry Service Swagger | `http://localhost:5250/swagger` |
| Alert Service Swagger | `http://localhost:5300/swagger` |

**Quick health checks:**

```powershell
curl http://localhost:5000/health
curl http://localhost:5300/health
```

> **If Docker reports `open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified`:** Docker Desktop is not running, or the Linux engine has not finished starting. Open Docker Desktop, wait until it says the engine is running, then rerun `docker compose up --build -d`.
>
> **If containers start and then fail:** Run `docker compose logs alert-service` or `docker compose logs race-service` from the `Lab-start` folder. Most container startup failures come from missing migrations, a port already being used, or service configuration problems.

---

## Step 11 — Test the Alert Workflow

### What You're Doing

You will create a race, add a race entry, and then verify Alert Service received an alert from Race Service.

This is the important microservices moment: **the alert is not created by the same process that receives your race-entry request.** Race Service receives the entry request, then calls Alert Service over HTTP.

> **Swagger note:** Gateway Swagger (`http://localhost:5000/swagger`) only shows endpoints owned by the gateway, such as `/health`. It does not automatically aggregate Swagger documents from downstream services. Use the individual service Swagger pages for interactive testing.

### Step 11a: Create a Race Through Race Service

Open Race Service Swagger:

- [`http://localhost:5200/swagger`](`http://localhost:5200/swagger)

Run:

- `POST /api/races`

Request body:

```json
{
  "name": "Workshop Sprint",
  "startTime": "2026-06-01T14:00:00Z",
  "trackName": "Monaco"
}
```

✅ **Success criteria:** response is `201 Created` and includes a race ID. Copy the race ID for the next step.

### Step 11b: Add a Race Entry Through the Gateway

In Race Service Swagger, run:

- `POST /api/races/{raceId}/entries`

Use the race ID from Step 11a.

Request body:

```json
{
  "driverId": "11111111-1111-1111-1111-111111111111",
  "carNumber": 7
}
```

✅ **Success criteria:** response is `201 Created`. This request triggers Race Service to call Alert Service.

### Step 11c: Verify Alert Service Received the Alert

Open Alert Service Swagger:

- [`http://localhost:5300/swagger`](http://localhost:5300/swagger)

Run:

- `GET /api/alerts`

You should see at least one alert. It should include the race ID and driver ID from the race entry.

Then run:

- `GET /api/alerts?raceId={raceId}`

Use the same race ID from Step 11a.

✅ **Success criteria:** the filtered response includes the alert created by the race-entry workflow.

### Step 11d: Acknowledge the Alert

Copy the alert ID from the `GET /api/alerts` response.

Run:

- `PUT /api/alerts/{id}/acknowledge`

✅ **Success criteria:** response is `200 OK` and `isAcknowledged` is `true`.

### Step 11e: Regression Check — Existing Services Still Work

Verify the rest of the platform still responds:

| Endpoint | Expected |
|---|---|
| Gateway `GET /health` | 200 OK |
| Race Service `GET /api/races` directly at [`http://localhost:5200/swagger`](http://localhost:5200/swagger) | 200 OK |
| Alert Service `GET /api/alerts` directly | 200 OK |
| Crew Service Swagger at [`http://localhost:5100/swagger`](http://localhost:5100/swagger) | Loads |
| Telemetry Service Swagger at [`http://localhost:5250/swagger`](http://localhost:5250/swagger) | Loads |

✅ **Success criteria:** completing Alert Service does not break existing services.

---

## Completion Checklist

- [ ] Starter solution builds before changes
- [ ] `Alert` model has race ID, driver ID, alert type, severity, message, issued timestamp, and acknowledgment state
- [ ] `AlertDbContext` exposes `DbSet<Alert> Alerts`
- [ ] Alert DTOs define create request and response shapes
- [ ] `AlertRepository` can create, list, filter, and update alerts
- [ ] `AlertService` maps domain entities to DTOs and acknowledges alerts
- [ ] `AlertsController` exposes POST, GET, filtered GET, acknowledge, and health endpoints
- [ ] `Program.cs` registers DbContext, repository, service, controllers, Swagger, and startup migrations
- [ ] Alert Service migration exists under `src\CiphersGrid.AlertService\Migrations\`
- [ ] Docker Compose starts gateway, crew, race, telemetry, and alert containers
- [ ] Adding a race entry creates an alert
- [ ] Alert can be retrieved and acknowledged

---

## Troubleshooting

### "dotnet ef is not recognized"

Install the EF Core CLI tool:

```powershell
dotnet tool install --global dotnet-ef --version 10.*
```

Then close and reopen your terminal if the command is still not found.

### "Alert Service startup fails with missing table"

Check that you completed Step 9 and that `src\CiphersGrid.AlertService\Migrations\` contains migration files.

`Program.cs` calls `MigrateAsync()` at startup, but it can only apply migrations that exist.

### "Docker says a port is already allocated"

Another process or a previous lab stack is already using one of the lab ports. Stop the existing stack from the folder where you started it:

```powershell
docker compose down
```

If you previously ran the lab from another copy, such as `Lab-end` or a backup folder, run `docker compose down` from that folder too. The lab uses host ports `5000`, `5100`, `5200`, `5250`, and `5300`, so only one copy can run at a time.

Then rerun:

```powershell
docker compose up --build -d
```

If old containers named `ciphers-gateway`, `ciphers-crew`, `ciphers-race`, `ciphers-telemetry`, or `ciphers-alerts` are still visible in Docker Desktop, stop and remove them before rerunning the lab. Newer lab Compose files do not use fixed container names, which prevents Lab-start and Lab-end copies from colliding.

### "Docker cannot connect to `dockerDesktopLinuxEngine`"

If you see an error like:

```text
open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified
```

Docker Desktop is not running, or its Linux engine is still starting.

**Fix:**

1. Open **Docker Desktop**.
2. Wait until Docker Desktop reports that the engine is running.
3. From `Sections\4-Microservices\Lab-start`, rerun:

   ```powershell
   docker compose up --build -d
   ```

The `version` warning from older Compose files is safe to ignore, but this lab's Compose files no longer include the obsolete `version` attribute.

### "Docker build fails with `MSB3202: The project file ... was not found`"

If Docker fails during a step like:

```text
RUN dotnet restore CiphersGrid.sln
error MSB3202: The project file "/src/src/CiphersGrid.Api/CiphersGrid.Api.csproj" was not found.
```

you are using an older Dockerfile. The service Dockerfiles should restore the specific service project, not the whole solution, because the Docker build stage only copies the current service project and `SharedKernel` before restore.

**Fix:** make sure the Dockerfiles use project-specific restore commands, for example:

```dockerfile
RUN dotnet restore src/CiphersGrid.AlertService/CiphersGrid.AlertService.csproj
```

Each service Dockerfile should restore its own `.csproj`:

- `src/CiphersGrid.Api/CiphersGrid.Api.csproj`
- `src/CiphersGrid.CrewService/CiphersGrid.CrewService.csproj`
- `src/CiphersGrid.RaceService/CiphersGrid.RaceService.csproj`
- `src/CiphersGrid.TelemetryService/CiphersGrid.TelemetryService.csproj`
- `src/CiphersGrid.AlertService/CiphersGrid.AlertService.csproj`

After updating, rerun:

```powershell
docker compose up --build -d
```

### "Race entry succeeds, but no alert appears"

Check the service-to-service path:

1. Confirm Alert Service is running: `http://localhost:5300/health`
2. Check Race Service logs:

   ```powershell
   docker compose logs race-service
   ```

3. Check Alert Service logs:

   ```powershell
   docker compose logs alert-service
   ```

4. Verify `docker-compose.yml` contains:

   ```yaml
   - ServiceUrls__AlertService=http://alert-service:8080
   ```

Race Service catches Alert Service failures so the race entry can still succeed. That is intentional graceful degradation, but it means you need logs to diagnose the missing alert.

### "POST /api/races returns 500 Internal Server Error"

If race creation returns 500, check the Race Service logs:

```powershell
docker compose logs race-service
```

If the logs mention `no such table: Races`, you are using an older lab copy where Race Service did not create its own SQLite schema at startup. Update to the latest lab files, rebuild, and start from clean volumes:

```powershell
docker compose down -v
docker compose up --build -d
```

The current lab files initialize the prebuilt Crew, Race, and Telemetry service databases automatically. Only the Alert Service migration is part of the participant lab work.

### "GET /api/alerts?raceId={raceId} returns empty"

Verify that:

- You used the race ID returned by `POST /api/races`
- You added an entry to that same race ID
- Alert Service created an alert in `GET /api/alerts`

If `GET /api/alerts` has alerts but the filtered endpoint is empty, the race ID in the query does not match the alert's `raceId`.

---

## Teaching Points

### What You've Learned

1. **Independent deployment units:** Each service runs in its own container and can be deployed independently.

2. **Database ownership:** Alert Service owns its own SQLite database. Other services do not query its tables directly.

3. **HTTP contracts replace in-process calls:** Race Service talks to Alert Service through `AlertServiceClient`, not direct project references.

4. **Service discovery:** Inside Docker Compose, services call each other by container service name, such as `http://alert-service:8080`.

5. **Failure isolation:** Race Service catches Alert Service failures so a missing alert does not prevent race entry creation.

6. **Operational complexity:** The feature spans code, containers, networking, configuration, migrations, and logs. That is the cost side of microservices.

### The Microservices Tradeoff

Compared with the modular monolith, microservices give stronger isolation and deployment independence. But the boundary is now a network boundary. You gain team autonomy, scalability options, and failure isolation, but you also take on service discovery, container orchestration, distributed debugging, and cross-service reliability concerns.

---

## Cleanup

When you are done testing, stop the platform:

```powershell
docker compose down
```

If you want to remove persisted lab databases too:

```powershell
docker compose down -v
```

Use `-v` only when you want a clean database on the next run.
