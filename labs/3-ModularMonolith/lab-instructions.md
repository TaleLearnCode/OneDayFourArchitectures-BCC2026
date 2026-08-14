# Module 3 Lab: Modular Monolith — Add the Penalties Feature

## Introduction: What Is a Modular Monolith, and Why Does It Matter?

After Module 1 (Monolith) and Module 2 (N-Tier), you've learned two deployment patterns. But there's a third way that bridges both worlds: the **modular monolith**.

A modular monolith keeps everything in one deployable unit (like the monolith), but enforces internal boundaries using separate class libraries and shared contracts (like N-Tier). Each module is owned by a team, owns its own database layer, and exposes itself through an interface. At composition time, all modules are wired at the application root via dependency injection.

This teaches a crucial insight: **Architecture isn't just about deployment.** You can have a monolithic *deployment* with a microservice *organization*, allowing teams to work independently on the same codebase without stepping on each other.

The cost: you're managing multiple `.csproj` files and shared contracts, but you're not managing service discovery, network latency, or distributed transactions.

**This lab is designed to make you experience module ownership, contract-first design, and composition at the application root.**

---

## What You're Building

**The Circuit** is a race management system. The scaffold already includes complete implementations of:

| Module | Responsibility |
|---|---|
| `TheCircuit.Events` | Race schedules and event management |
| `TheCircuit.Participants` | Racer registration and tracking |
| `TheCircuit.Results` | Race results and time calculations |
| `TheCircuit.SharedKernel` | Contracts, enums, IDs, DTOs shared by all modules |

What The Circuit doesn't have yet is a **Penalties module** — a way to issue time penalties to racers for rule violations and automatically apply those penalties to their final times.

You're going to add it from scratch: a new module that owns Penalties data, exposes a Penalties service, and integrates with Results to adjust race times. All while leaving Events, Participants, and Results completely untouched.

By the end of this lab, you will have:

- Added a new module project (`TheCircuit.Penalties`) with repository and service patterns
- Designed Penalties to work with SharedKernel contracts and enums
- Called a cross-module service (Results) from Penalties to apply penalty effects
- Exposed three REST endpoints under `api/events/{eventId}/penalties`
- Verified the feature works end-to-end and doesn't break existing modules
- Experienced how modular monolith teams can ship independently with compile-time boundaries

That's a complete feature integrated into an existing system without modifying any existing modules.

---

## Getting Started

**Project location:** `lab/3-ModularMonolith/Lab-start/`

### Step 0 — Verify the Starter State

Before you add Penalties, verify that the existing modules work correctly.

**From a terminal in `labs/3-ModularMonolith/lab-start`:**

```bash
dotnet build TheCircuit.sln
dotnet run --project src/TheCircuit.Api/TheCircuit.Api.csproj
```

Then navigate to the Swagger URL shown in your terminal output (commonly [`http://localhost:5001/swagger`](http://localhost:5001/swagger) or similar).

**You should see endpoint groups for:**
- **Events** — Manage race events
- **Participants** — Manage racers
- **Results** — Retrieve race results
- **Health** — Basic health check

**You should NOT see Penalties endpoints yet** — that's what you're adding.

**Verify starter functionality with a quick smoke test:**

| Endpoint | Expected | Notes |
|---|---|---|
| `GET /api/events` | 200 OK, list of events | Read-only in starter; Events module is complete |
| `GET /api/participants` | 200 OK, list of racers | Read-only in starter |
| `GET /api/results/events/1` | 200 OK, list of results for event 1 | Verify there are results; you'll check penalty-adjusted times later |
| `GET /health` | 200 OK | Sanity check for API |

> **If the app won't start:** Run `dotnet restore` to ensure all NuGet packages are present. Verify you're in the correct directory (with `TheCircuit.sln`).

---

## Step 1 — Learn the Module Pattern by Example

### What You're Doing

Before you build Penalties, understand how existing modules are structured. This is the template you'll follow.

### Step 1a: Examine an Existing Module Structure

**File locations:** `src/TheCircuit.Events/`

Pick any complete module (Events, Participants, or Results) and note its structure:

```
TheCircuit.Events/
├── TheCircuit.Events.csproj
├── Data/
│   └── EventsDbContext.cs        — Entity Framework context (internal)
├── Models/
│   └── Event.cs                  — Internal data model (not exported)
├── Repositories/
│   └── EventRepository.cs         — Data access layer
├── Services/
│   └── EventService.cs            — Business logic, implements IEventService
├── Migrations/
│   └── [auto-generated]           — EF Core migrations
└── EventsModule.cs                — Self-registration class
```

### Step 1b: Examine the `EventsModule` Pattern — The Key to Composability

**File location:** `src/TheCircuit.Events/EventsModule.cs`

Every module follows this exact pattern:

```csharp
public static class EventsModule
{
    public static IServiceCollection AddEventsModule(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register DbContext with dependency injection
        services.AddDbContext<EventsDbContext>(...);
        
        // Register repository and service interfaces
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventService, EventService>();
        
        return services;
    }

    public static async Task InitializeEventsAsync(IServiceProvider services)
    {
        // Run migrations, seed data, etc.
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
        await context.Database.MigrateAsync();
    }
}
```

**This pattern is critical.** It means:
- Each module registers itself — the API layer doesn't need to know internal details
- Adding a new module is just two lines in `Program.cs`: `AddPenaltiesModule()` and `InitializePenaltiesAsync()`
- That's the modular monolith: independent modules that know how to plug themselves in

### Step 1c: Examine SharedKernel Contracts (What All Modules Depend On)

**File location:** `src/TheCircuit.SharedKernel/Contracts/IPenaltyService.cs`

Open this file. This is the interface your Penalties module will implement. It defines:

```csharp
public interface IPenaltyService
{
    Task<PenaltyDto?> GetPenaltyByIdAsync(PenaltyId id);
    Task<IEnumerable<PenaltyDto>> GetPenaltiesByEventAsync(EventId eventId);
    Task<PenaltyDto> IssuePenaltyAsync(PenaltyDto penaltyDto);
}
```

**Also examine these SharedKernel artifacts** (they're already written for you):
- `src/TheCircuit.SharedKernel/DTOs/PenaltyDto.cs` — The DTO your service will return
- `src/TheCircuit.SharedKernel/Enums/PenaltyReason.cs` — Enum for penalty types (`CourseCut`, `FalseStart`, etc.)
- `src/TheCircuit.SharedKernel/Enums/PenaltyStatus.cs` — Enum for penalty status (Issued, Applied, Revoked)
- `src/TheCircuit.SharedKernel/Ids/PenaltyId.cs`, `EventId.cs`, `RacerId.cs` — Strongly-typed IDs
- `src/TheCircuit.SharedKernel/DTOs/IssuePenaltyRequest.cs` — Request body shape for the POST endpoint

All of these are already in `Lab-start/TheCircuit.SharedKernel/`. You don't write these — you use them.

### Step 1d: Verify `Program.cs` Registration Pattern

**File location:** `src/TheCircuit.Api/Program.cs`

Find the section where modules are registered:

```csharp
builder.Services.AddEventsModule(builder.Configuration);
builder.Services.AddParticipantsModule(builder.Configuration);
builder.Services.AddResultsModule(builder.Configuration);
```

And the initialization section (after `app.Build()`):

```csharp
await TheCircuit.Events.EventsModule.InitializeEventsAsync(app.Services);
await TheCircuit.Participants.ParticipantsModule.InitializeParticipantsAsync(app.Services);
await TheCircuit.Results.ResultsModule.InitializeResultsAsync(app.Services);
```

You'll add two identical lines for Penalties here.

---

## Step 2 — Create the Penalties Module Project

### What You're Doing

The Penalties module doesn't exist yet in lab-start. You'll create it from scratch (or copy from lab-end as a template) and add it to the solution.

### Step 2a: Create the Project

From the `labs/3-ModularMonolith/lab-start/src/` directory:

```bash
dotnet new classlib -n TheCircuit.Penalties
```

This creates a new class library project.

> [!TIP]
>
> If you are using Visual Studio, you could also just add the `TheCircuit.Penalities` class library project to the solution within the Solution Explorer window. If doing so, you can skip Step 2b.

### Step 2b: Add the Project to the Solution

**File location:** `labs/3-ModularMonolith/lab-start/TheCircuit.sln`

From the `lab-start` root directory:

```bash
dotnet sln TheCircuit.sln add src/TheCircuit.Penalties/TheCircuit.Penalties.csproj
```

### Step 2c: Add Project References

The Penalties module needs to reference **SharedKernel** (for DTOs, enums, contracts) and **Results** (for cross-module service call).

**In `src/TheCircuit.Penalties/TheCircuit.Penalties.csproj`:**

```xml
<ItemGroup>
    <ProjectReference Include="..\TheCircuit.SharedKernel\TheCircuit.SharedKernel.csproj" />
    <ProjectReference Include="..\TheCircuit.Results\TheCircuit.Results.csproj" />
</ItemGroup>
```

**In `src/TheCircuit.Api/TheCircuit.Api.csproj`:**

Add a reference so the API can call Penalties:

```xml
<ItemGroup>
    <ProjectReference Include="..\TheCircuit.Penalties\TheCircuit.Penalties.csproj" />
</ItemGroup>
```

### Step 2d: Add Required NuGet Dependencies

**In `src/TheCircuit.Penalties/TheCircuit.Penalties.csproj`:**

Add Entity Framework Core:

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.11" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.11" />
</ItemGroup>
```

(Match the same latest stable versions used by the other modules in the solution.)

---

## Step 3 — Create Module Files

### What You're Doing

Create the core files that every module needs:
- **Models/Penalty.cs** — Internal data model
- **Data/PenaltiesDbContext.cs** — Entity Framework context
- **Repositories/PenaltyRepository.cs** — Data access layer
- **Services/PenaltyService.cs** — Business logic, implements IPenaltyService
- **PenaltiesModule.cs** — Self-registration

### Step 3a: Create Folder Structure

In `src/TheCircuit.Penalties/`, create these folders (if they don't exist from dotnet new):

```
src/TheCircuit.Penalties/
├── Data/
├── Models/
├── Repositories/
└── Services/
```

### Step 3b: Create `Models/Penalty.cs`

This is the internal data model for the database.

```csharp
using TheCircuit.SharedKernel.Enums;

namespace TheCircuit.Penalties.Models;

internal class Penalty
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int RacerId { get; set; }
    public PenaltyReason Reason { get; set; }
    public int PenaltySeconds { get; set; }
    public DateTime IssuedAt { get; set; }
    public required string OfficialNotes { get; set; }
    public PenaltyStatus Status { get; set; }
}
```

**Key points:**
- Uses SharedKernel enums (`PenaltyReason`, `PenaltyStatus`), not local duplicates
- Internal class (not exported; only service exposes behavior)
- Maps to the `Penalties` database table

### Step 3c: Create `Data/PenaltiesDbContext.cs`

This is the Entity Framework context.

```csharp
using Microsoft.EntityFrameworkCore;
using TheCircuit.Penalties.Models;
using TheCircuit.SharedKernel.Enums;

namespace TheCircuit.Penalties.Data;

internal class PenaltiesDbContext : DbContext
{
    public PenaltiesDbContext(DbContextOptions<PenaltiesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Penalty> Penalties => Set<Penalty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Penalty>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.EventId).IsRequired();
            entity.Property(p => p.RacerId).IsRequired();
            entity.Property(p => p.Reason).HasConversion<int>();
            entity.Property(p => p.Status).HasConversion<int>();
            entity.Property(p => p.OfficialNotes).IsRequired().HasMaxLength(500);
        });
    }
}
```

**Key points:**
- Configures the Penalty entity: constraints, conversions, max lengths
- Enums stored as `int` in the database

### Step 3d: Create `Repositories/PenaltyRepository.cs`

This handles all data access.

```csharp
using TheCircuit.Penalties.Data;
using TheCircuit.Penalties.Models;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Penalties.Repositories;

internal class PenaltyRepository
{
    private readonly PenaltiesDbContext _context;

    public PenaltyRepository(PenaltiesDbContext context)
    {
        _context = context;
    }

    public async Task<Penalty?> GetByIdAsync(PenaltyId id)
    {
        return await _context.Penalties.FindAsync(id.Value);
    }

    public IQueryable<Penalty> GetAll()
    {
        return _context.Penalties.AsQueryable();
    }

    public IQueryable<Penalty> GetByEventId(EventId eventId)
    {
        return _context.Penalties.Where(p => p.EventId == eventId.Value);
    }

    public async Task<Penalty> AddAsync(Penalty penalty)
    {
        _context.Penalties.Add(penalty);
        await _context.SaveChangesAsync();
        return penalty;
    }

    public async Task UpdateAsync(Penalty penalty)
    {
        _context.Penalties.Update(penalty);
        await _context.SaveChangesAsync();
    }
}
```

**Key points:**
- Uses strongly-typed IDs (`PenaltyId`, `EventId`) from SharedKernel
- Returns `IQueryable<Penalty>` for filtering (important for querying by event)

### Step 3e: Create `Services/PenaltyService.cs`

This implements `IPenaltyService` and contains business logic.

```csharp
using Microsoft.EntityFrameworkCore;
using TheCircuit.Penalties.Data;
using TheCircuit.Penalties.Models;
using TheCircuit.Penalties.Repositories;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.Enums;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Penalties.Services;

internal class PenaltyService : IPenaltyService
{
	private readonly PenaltyRepository _repository;
	private readonly IResultsService _resultsService;

	public PenaltyService(PenaltiesDbContext context, IResultsService resultsService)
	{
		_repository = new PenaltyRepository(context);
		_resultsService = resultsService;
	}

	public async Task<PenaltyDto?> GetPenaltyByIdAsync(PenaltyId id)
	{
		var penalty = await _repository.GetByIdAsync(id);
		return penalty is null ? null : MapToDto(penalty);
	}

	public async Task<IEnumerable<PenaltyDto>> GetPenaltiesByEventAsync(EventId eventId)
	{
		var penalties = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
			.ToListAsync(_repository.GetByEventId(eventId));
		return penalties.Select(MapToDto);
	}

	public async Task<PenaltyDto> IssuePenaltyAsync(PenaltyDto penaltyDto)
	{
		var penalty = new Penalty
		{
			EventId = penaltyDto.EventId.Value,
			RacerId = penaltyDto.RacerId.Value,
			Reason = penaltyDto.Reason,
			PenaltySeconds = penaltyDto.PenaltySeconds,
			IssuedAt = penaltyDto.IssuedAt,
			OfficialNotes = penaltyDto.OfficialNotes,
			Status = PenaltyStatus.Issued
		};

		var saved = await _repository.AddAsync(penalty);

		// Apply penalty to race results
		await _resultsService.ApplyPenaltyAsync(
				penaltyDto.EventId,
				penaltyDto.RacerId,
				penaltyDto.PenaltySeconds
		);

		// Mark as applied
		saved.Status = PenaltyStatus.Applied;
		await _repository.UpdateAsync(saved);

		return MapToDto(saved);
	}

	private PenaltyDto MapToDto(Penalty penalty)
	{
		return new PenaltyDto(
				new PenaltyId(penalty.Id),
				new EventId(penalty.EventId),
				new RacerId(penalty.RacerId),
				penalty.Reason,
				penalty.PenaltySeconds,
				penalty.IssuedAt,
				penalty.OfficialNotes,
				penalty.Status
		);
	}
}
```

**Key points:**
- Implements `IPenaltyService` (the SharedKernel contract)
- Maps internal `Penalty` model to public `PenaltyDto`
- Calls `_resultsService.ApplyPenaltyAsync()` to integrate with Results module (cross-module call)
- Uses strongly-typed IDs consistently

### Step 3f: Create `PenaltiesModule.cs`

This is the self-registration class.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheCircuit.Penalties.Data;
using TheCircuit.Penalties.Services;
using TheCircuit.SharedKernel.Contracts;

namespace TheCircuit.Penalties;

public static class PenaltiesModule
{
    public static IServiceCollection AddPenaltiesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<PenaltiesDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=Penalties.db")
        );

        // Register service
        services.AddScoped<IPenaltyService, PenaltyService>();

        return services;
    }

    public static async Task InitializePenaltiesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PenaltiesDbContext>();
        await context.Database.MigrateAsync();
    }
}
```

**Key points:**
- Follows the same pattern as EventsModule, ParticipantsModule, ResultsModule
- Registers both DbContext and service interface
- `InitializePenaltiesAsync()` runs migrations on startup

---

## Step 4 — Create `PenaltiesController.cs`

### What You're Doing

The API layer exposes the Penalties service via REST endpoints.

**File location:** `src/TheCircuit.Api/Controllers/PenaltiesController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.Enums;
using TheCircuit.SharedKernel.Ids;
using CircuitEventId = TheCircuit.SharedKernel.Ids.EventId;

namespace TheCircuit.Api.Controllers;

[ApiController]
[Route("api/events/{eventId}/[controller]")]
public class PenaltiesController : ControllerBase
{
	private readonly IPenaltyService _penaltyService;

	public PenaltiesController(IPenaltyService penaltyService)
	{
		_penaltyService = penaltyService;
	}

	[HttpGet]
	public async Task<IActionResult> GetEventPenalties(int eventId)
	{
		var penalties = await _penaltyService.GetPenaltiesByEventAsync(new CircuitEventId(eventId));
		return Ok(penalties);
	}

	[HttpPost]
	public async Task<IActionResult> IssuePenalty(int eventId, [FromBody] IssuePenaltyRequest request)
	{
		var penaltyDto = new PenaltyDto(
				new PenaltyId(0),
				new CircuitEventId(eventId),
				new RacerId(request.RacerId),
				request.Reason,
				request.PenaltySeconds,
				DateTime.UtcNow,
				request.OfficialNotes,
				PenaltyStatus.Issued
		);

		var result = await _penaltyService.IssuePenaltyAsync(penaltyDto);
		return CreatedAtAction(nameof(GetPenalty), new { eventId, penaltyId = result.Id.Value }, result);
	}

	[HttpGet("{penaltyId}")]
	public async Task<IActionResult> GetPenalty(int eventId, int penaltyId)
	{
		var penalty = await _penaltyService.GetPenaltyByIdAsync(new PenaltyId(penaltyId));
		return penalty is null ? NotFound() : Ok(penalty);
	}
}

public class IssuePenaltyRequest
{
	public int RacerId { get; set; }
	public PenaltyReason Reason { get; set; }
	public int PenaltySeconds { get; set; }
	public required string OfficialNotes { get; set; }
}
```

**Key points:**
- Route: `api/events/{eventId}/penalties` — all endpoints are event-scoped
- Constructor injects `IPenaltyService` (interface, not concrete class)
- POST returns 201 Created with `CreatedAtAction()`
- Request model maps to `PenaltyDto` before calling service

---

## Step 5 — Wire Module Registration in `Program.cs`

### What You're Doing

Add two lines to `Program.cs` to register and initialize the Penalties module.

### Step 5a: Add Module Reference

**File:** `src/TheCircuit.Api/Program.cs`

Add the following the using statements at the top of the file:

```csharp
using TheCircuit.Penalties;
```

### Step 5b: Add Module Registration

**File:** `src/TheCircuit.Api/Program.cs`

Find the section where other modules are registered and add this line:

```csharp
builder.Services.AddPenaltiesModule(builder.Configuration);
```

### Step 5c: Add Module Initialization

**File:** `src/TheCircuit.Api/Program.cs`

Find the initialization section (after `app.Build()`) and add this line:

```csharp
await TheCircuit.Penalties.PenaltiesModule.InitializePenaltiesAsync(app.Services);
```

### Step 5d: Create the Penalties Database Migration

**Critical:** Before running the API, create an EF Core migration for the Penalties module. Otherwise, `InitializePenaltiesAsync()` will have no schema to apply, and Step 7a will fail with:
```
SQLite Error 1: 'no such table: Penalties'
```

**Why?** 
- `PenaltiesDbContext.OnModelCreating()` *defines* the schema (table structure, constraints)
- A migration *translates* that definition into SQL
- `InitializePenaltiesAsync()` at startup calls `context.Database.MigrateAsync()` to apply the migration
- Without a migration file, there's nothing to apply, so the table never gets created

**Step 5d-i: Ensure dotnet ef CLI is Installed**

```powershell
dotnet tool install --global dotnet-ef
```

(Already installed? Run `dotnet tool update --global dotnet-ef` to ensure it's current.)

**Step 5d-ii: Create the Migration**

From `labs/3-ModularMonolith/Lab-start` (the solution root), run:

```powershell
dotnet ef migrations add AddPenalties `
  --project src\TheCircuit.Penalties\TheCircuit.Penalties.csproj `
  --startup-project src\TheCircuit.Api\TheCircuit.Api.csproj `
  --context PenaltiesDbContext
```

The Lab-start API project already includes `Microsoft.EntityFrameworkCore.Design`, which EF Core needs because `TheCircuit.Api` is the startup project for this command. The `--context` option tells EF Core to create the migration for the new Penalties module instead of the existing Events, Participants, or Results contexts.

**What this does:**
- Analyzes `PenaltiesDbContext` and `OnModelCreating()` configuration
- Compares to any existing migrations (none yet)
- Generates a migration file: `src\TheCircuit.Penalties\Migrations\YYYYMMDDHHMMSS_AddPenalties.cs`
- That file contains code to create the Penalties table with all required columns

**Expected output:**
```
Done. To undo this action, use 'ef migrations remove'
```

**Step 5d-iii: Verify**

Check that `src\TheCircuit.Penalties\Migrations\` now exists and contains:
- `PenaltiesDbContextModelSnapshot.cs` — Metadata snapshot
- `YYYYMMDDHHMMSS_AddPenalties.cs` — The migration itself

The migration file should contain a `CreateTable("Penalties", ...)` call with columns: `Id`, `EventId`, `RacerId`, `Reason`, `PenaltySeconds`, `IssuedAt`, `OfficialNotes`, `Status`.

> **Note:** You do **not** run `dotnet ef database update` manually. When the API starts, `InitializePenaltiesAsync()` in `Program.cs` calls `context.Database.MigrateAsync()`, which automatically applies all pending migrations to the database.

---

## Step 6 — Build and Verify Endpoints

### What You're Doing

Build the solution and confirm Penalties endpoints appear in Swagger.

### Step 6a: Build the Solution

**From `labs/3-ModularMonolith/lab-start`:**

```bash
dotnet build TheCircuit.sln
```

If the build fails, check:
- Are all project references correct? (Penalties → SharedKernel, Penalties → Results, Api → Penalties)
- Does `PenaltyService` inject `IResultsService`?
- Are all namespaces for SharedKernel DTOs, enums, and IDs present?

### Step 6b: Run the API

**From `labs/3-ModularMonolith/lab-start`:**

```bash
dotnet run --project src/TheCircuit.Api/TheCircuit.Api.csproj
```

### Step 6c: Verify Endpoints Appear in Swagger

Navigate to the Swagger URL (e.g., [`http://localhost:5001/swagger`](http://localhost:5001/swagger)).

**New endpoint group should now appear:**
- **Penalties** — with three operations:
  - `GET /api/events/{eventId}/penalties` — List penalties for an event
  - `POST /api/events/{eventId}/penalties` — Issue a new penalty
  - `GET /api/events/{eventId}/penalties/{penaltyId}` — Get a specific penalty

If Penalties endpoints are missing:
- Verify both registration lines were added to `Program.cs`
- Verify `PenaltiesController.cs` is in `src/TheCircuit.Api/Controllers/`
- Restart the API

---

## Step 7 — Test Penalties Workflow

### What You're Doing

Test the full Penalties feature end-to-end: issue a penalty, retrieve it, and verify it was applied to race results.

### Step 7a: Issue a Penalty (POST)

In Swagger, expand **Penalties** → **POST /api/events/{eventId}/penalties**

Set `eventId` to `1` and use this request body:

```json
{
  "racerId": 1,
  "reason": "CourseCut",
  "penaltySeconds": 5,
  "officialNotes": "Cut corner at turn 3"
}
```

**Expected response:** 201 Created with a response body containing:

```json
{
  "id": {
    "value": 1
  },
  "eventId": {
    "value": 1
  },
  "racerId": {
    "value": 1
  },
  "reason": "CourseCut",
  "penaltySeconds": 5,
  "issuedAt": "2026-08-11T20:12:00Z",
  "officialNotes": "Cut corner at turn 3",
  "status": "Applied"
}
```

✅ **Success criteria:** Response is 201 Created, contains the penalty with status `Applied`, and data matches the request.

### Step 7b: Retrieve Penalties by Event (GET)

In Swagger, expand **Penalties** → **GET /api/events/{eventId}/penalties**

Set `eventId` to `1`.

**Expected response:** 200 OK with an array containing the penalty you just created.

✅ **Success criteria:** Response is 200 OK and includes the penalty from Step 7a.

### Step 7c: Retrieve a Specific Penalty (GET)

In Swagger, expand **Penalties** → **GET /api/events/{eventId}/penalties/{penaltyId}**

Use `eventId = 1` and `penaltyId = 1` (from the POST response).

**Expected response:** 200 OK with the penalty details.

✅ **Success criteria:** Response is 200 OK and penalty data matches.

### Step 7d: Verify Penalty Integration with Results

This is the cross-module call. When a penalty is issued, `PenaltyService` calls `IResultsService.ApplyPenaltyAsync(...)` to adjust the racer's race time.

**From Swagger, test the Results endpoint:**

- Navigate to **Results** → **GET /api/events/1/Results**
- Look for the racer's result (same `racerId` as the penalty you issued)
- Verify the penalty is applied: the Results response does **not** include an explicit `penalties` field, but the penalty is evident in the calculation: **`adjustedTimeMs = lapTimeMs + (penaltySeconds × 1000)`**
  - Example: if `lapTimeMs: 125000` and you issued a 5-second penalty, expect `adjustedTimeMs: 130000` (125000 + 5000)

✅ **Success criteria:** Penalty persists in Penalties endpoints AND Results endpoint shows the adjusted time (with `adjustedTimeMs` > `lapTimeMs`).

### Step 7e: Regression Test — Existing Modules Still Work

Verify nothing is broken:

- `GET /api/events` — Should return 200 OK (unchanged)
- `GET /api/participants` — Should return 200 OK (unchanged)
- `GET /health` — Should return 200 OK

✅ **Success criteria:** All existing endpoints return 200 OK; no functionality is broken.

---

## Completion Checklist

- [ ] Existing modules (Events, Participants, Results) build and run successfully
- [ ] `AddPenaltiesModule()` registered in `Program.cs`
- [ ] `InitializePenaltiesAsync()` called in application startup
- [ ] Penalties endpoints appear in Swagger UI
- [ ] POST `/api/events/{eventId}/penalties` returns 201 Created
- [ ] GET `/api/events/{eventId}/penalties` returns list of penalties
- [ ] GET `/api/events/{eventId}/penalties/{penaltyId}` returns single penalty
- [ ] Penalty is applied to Results (race time adjusted, or status updated, depending on implementation)
- [ ] All existing endpoints (Events, Participants, Results) still work (regression test)

---

## Troubleshooting

### "Build fails with 'project reference not found' or 'missing ProjectReference'"

**Check:**
1. Did you add the project reference to `Penalties` from the `Api` project `.csproj`?
2. Did you add `TheCircuit.Penalties.csproj` to the solution? (`dotnet sln add ...`)
3. Is the path correct in the project reference? (Should be `../TheCircuit.Penalties/TheCircuit.Penalties.csproj`)

**Fix:** Verify project references are in place:
- `src/TheCircuit.Api/TheCircuit.Api.csproj` → `..\TheCircuit.Penalties\TheCircuit.Penalties.csproj`
- `src/TheCircuit.Penalties/TheCircuit.Penalties.csproj` → `..\TheCircuit.SharedKernel\TheCircuit.SharedKernel.csproj`
- `src/TheCircuit.Penalties/TheCircuit.Penalties.csproj` → `..\TheCircuit.Results\TheCircuit.Results.csproj`

Then run `dotnet build TheCircuit.sln` from `Lab-start`.

### "Penalties endpoints don't appear in Swagger"

**Check:**
1. Did you add `builder.Services.AddPenaltiesModule(builder.Configuration);` in `Program.cs`?
2. Did you add `await TheCircuit.Penalties.PenaltiesModule.InitializePenaltiesAsync(app.Services);` in `Program.cs`?
3. Is `PenaltiesController.cs` in the `src/TheCircuit.Api/Controllers/` folder?
4. Did you restart the API after editing `Program.cs`?

**Fix:** Verify both registration lines are in `Program.cs`, restart the API, then refresh Swagger.

### "Build fails with 'PenaltyService' or 'PenaltiesModule' compilation error"

**Check:**
1. Is `PenaltyService` correctly implementing `IPenaltyService`?
2. Does `PenaltyService` constructor inject `IResultsService`?
3. Are all namespaces correct? (Check imports for SharedKernel enums, IDs, DTOs)
4. Does `PenaltiesModule` have all required `using` statements?

**Fix:** Compare your code with `Lab-end/src/TheCircuit.Penalties/` to verify method signatures and namespaces. Common issues:
- Missing `using TheCircuit.SharedKernel.Ids;`
- Missing `using TheCircuit.SharedKernel.Enums;`
- `PenaltyService` constructor missing `IResultsService` parameter

### "POST /penalties returns 500 Internal Server Error"

**Check:**
1. Did `dotnet run` complete successfully? (Migrations should have run.)
2. Are database migrations applied? Check startup logs for errors like "migration failed."
3. Are required fields provided in the POST request body? (EventId, RacerId, OfficialNotes)
4. Does `PenaltiesDbContext.OnModelCreating()` configure the Penalty entity correctly?

**Fix:** Verify the POST request includes all required fields. Check application startup logs. If migrations fail, ensure `PenaltiesDbContext` uses the correct table name and column constraints.

### "Penalty persists but Results endpoint doesn't show adjusted time"

**Check:**
1. Does `PenaltyService.IssuePenaltyAsync()` call `_resultsService.ApplyPenaltyAsync(...)`?
2. Is `IResultsService` injected into the `PenaltyService` constructor?

**Fix:** Ensure `PenaltyService.IssuePenaltyAsync()` includes:
```csharp
await _resultsService.ApplyPenaltyAsync(
    penaltyDto.EventId,
    penaltyDto.RacerId,
    penaltyDto.PenaltySeconds
);
```

### "GET /penalties returns empty list even after successful POST"

**Check:**
1. Did the POST return 201 Created? (Verify in response status/body.)
2. Are you querying the correct `eventId` in the GET request?
3. Is the penalty table in the Penalties database? (Each module has its own context and database file/connection.)

**Fix:** Verify POST was successful (201 response). Use the same eventId in GET. If still empty, check that the Penalties database exists and has the penalty records (look for `.db` file or check connection string).

---

## Teaching Points

### What You've Learned

1. **Module Ownership:** Each module (`Events`, `Participants`, `Results`, `Penalties`) owns its own database, repository, and service. Changes to one module's internals don't require changes to others.

2. **Shared Contracts:** All modules depend on `SharedKernel` interfaces and DTOs. These contracts are the boundaries. As long as a module implements its interface, it can change anything inside without breaking the system.

3. **Strongly-Typed IDs:** Instead of passing plain `int` IDs around, modules use strongly-typed ID classes (`EventId`, `RacerId`, `PenaltyId`). This prevents accidental mismatches and makes the code self-documenting.

4. **Cross-Module Calls:** Penalties calls Results via `IResultsService` to apply penalty effects. This is the only coupling between modules — everything flows through interfaces, not concrete classes.

5. **Composition at the Root:** The API layer (`Program.cs`) is the only place where modules are wired together. Each module registers itself; the root doesn't need to know the internal structure.

6. **Monolith Deployment, Microservice Organization:** You deployed one application, one database per module, and one transaction boundary (within events). But each team owns its module independently, with compile-time boundaries. That's the modular monolith.

---

## Next Steps (When Ready for Lab-end Comparison)

To verify your implementation matches the reference, open `Sections/3-ModularMonolith/Lab-end/` and compare:

- **PenaltiesController** — Route pattern, action signatures, response types
- **PenaltyService** — Constructor, method implementations, cross-module calls
- **PenaltiesModule** — Registration and initialization pattern
- **Program.cs** — Where modules are wired

If your implementation differs, it might still be correct — there are multiple valid ways to structure a module. The key is that:
- All endpoints work and return expected data
- No other modules are modified
- The module follows the same pattern as Events, Participants, Results
- The solution builds and runs without errors
