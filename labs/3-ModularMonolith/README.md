# Module 3: Modular Monolith — The Circuit (Penalties Module)

![The Circuit: Modular Monolith Architecture Lab](the-circuit.webp)

## Overview

A **Modular Monolith** reorganizes an application around business domains or features instead of technical layers. Each module (Events, Participants, Results, and Penalties) is a self-contained feature boundary with its own logic, data models, and API surface. Modules are still packaged as a single deployable unit, but they communicate through explicit, decoupled contracts and interfaces. This structure makes the codebase scale conceptually even as it remains a monolith operationally.

Module 3 builds on the layering introduced in Module 2 by adding a second dimension: horizontal decomposition by domain. You will examine **The Circuit**, a race event management system organized into independent modules, then build the **Penalties module** from scratch and experience the architectural moment when a new domain can be added to a running system without modifying existing code.

The exercise highlights the core value of modular monoliths: independent development of loosely-coupled features using dependency injection and interface-based contracts, all within a single deployment unit. The debrief then asks: *"What would happen if we needed to scale the Penalties module independently? Or deploy it to a separate team?"* This question bridges to Module 4.

---

## What We're Building: The Circuit Race Event System

**The Circuit** is a modular monolith for managing race events, participants, results, and penalties. The scaffold includes four pre-built modules:

| Module | Responsibility | Status |
|---|---|---|
| **Events** | Race event scheduling and lifecycle | ✅ Complete |
| **Participants** | Racer registration and profiles | ✅ Complete |
| **Results** | Race results, timing, and penalty application | ✅ Complete |
| **Penalties** | Issue penalties, track penalty status | 🚧 Participant builds this |

Each module:
- Owns its data model and database context
- Implements a service interface defined in SharedKernel
- Communicates with other modules through dependency injection and interfaces, never direct database coupling
- Is registered in Program.cs via an extension method (e.g., `AddPenaltiesModule()`)

The **SharedKernel** contains:
- Strongly-typed IDs (`EventId`, `RacerId`, `ResultId`, `PenaltyId`)
- Service interfaces (`IEventService`, `IParticipantService`, `IResultsService`, `IPenaltyService`)
- DTOs and enums shared across module boundaries

**The lab task:** Implement the Penalties module from scratch, including the entity model, database context, repository, service, and REST API endpoints. The Penalties module demonstrates cross-module communication by calling `IResultsService.ApplyPenaltyAsync()` to update race results when a penalty is issued.

**Tech stack:** .NET 10, ASP.NET Core, Entity Framework Core, SQLite, Swagger/OpenAPI

---

## Module Contents

| Item | Description |
|---|---|
| [Lab Instructions](lab-instructions.md) | Step-by-step participant lab guide. Covers building the Penalties module from scratch, implementing module boundaries, cross-module communication patterns, and endpoint exposure. |
| [Lab Start](lab-start/) | Scaffold codebase which provides the starting point for this lab. |
| [Lab End](lab-end) | The fully implemented application including the Penalties feature built during the lab. |

---

## Key Architectural Patterns

### Module Registration via Extension Methods

Each module provides a static extension method for dependency injection:

```csharp
// In PenaltiesModule.cs
public static IServiceCollection AddPenaltiesModule(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddDbContext<PenaltiesDbContext>(options =>
        options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
    
    services.AddScoped<PenaltyRepository>();
    services.AddScoped<IPenaltyService, PenaltyService>();
    
    return services;
}
```

Used in Program.cs:
```csharp
builder.Services.AddPenaltiesModule(builder.Configuration);
```

### Cross-Module Communication (Penalties → Results)

Modules depend on interfaces, not implementations:

```csharp
internal class PenaltyService : IPenaltyService
{
    private readonly IResultsService _resultsService;

    public PenaltyService(IResultsService resultsService) { }
}
```

**Why this matters:**
- Penalties module cannot reference Results project directly
- But it can call Results functionality via the interface
- Eliminates compile-time coupling; modules can evolve independently

### Strongly-Typed IDs

Instead of naked `int` IDs, use strongly-typed value objects:

```csharp
// Bad: mixed EventId with RacerId by accident
public async Task<PenaltyDto?> GetPenaltyAsync(int id) { }

// Good: type-safe
public async Task<PenaltyDto?> GetPenaltyAsync(PenaltyId id) { }
```

### Shared Database with Multiple DbContexts

All modules share one SQLite database but have independent `DbContexts`:

- `EventsDbContext` → events table
- `ParticipantsDbContext` → racers table
- `ResultsDbContext` → race_results table
- `PenaltiesDbContext` → penalties table

Each module owns its schema; cross-module queries go through service interfaces, not shared contexts.

---

## Getting Started

**Prerequisites — participants should have the following installed before the session:**

- .NET 10 SDK ([download](https://dotnet.microsoft.com/download))
- A code editor (Visual Studio 2022+, VS Code with C# Dev Kit, or JetBrains Rider)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`
- Git (for cloning the scaffold repository)

**Get started:**

Refer to the [Lab Instructions](lab-instructions.md) for step-by-step setup and implementation guidance. The lab walks you through building the Penalties module using the same pattern as the pre-built modules, integrating with Results via `IResultsService`, and exposing REST endpoints.

**What you'll learn:**

- What a Modular Monolith is and how feature-based decomposition improves scalability
- How module boundaries prevent tangling and enable parallel development
- How to design interfaces and contracts for decoupled cross-module communication
- How to extend a modular monolith by adding a new feature module from scratch
- Where deployment boundaries emerge as modules grow independent and how that sets up microservices
