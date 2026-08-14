# Monolith Architecture Lab — Dom's Garage `ServiceRecord` Feature

## Introduction: What Is a Monolith, and Why Does It Matter?

A **monolithic architecture** is the most natural starting point for any software system. Everything lives in a single deployable unit; one project, one database, one deployment pipeline. Every feature shares the same process, the same codebase, and the same data store.

This isn't a legacy mistake. It's a deliberate tradeoff. Monoliths are:

- **Fast to Develop:** No service boundaries to negotiate, no network calls to wire up, no distributed coordination to reason about.
- **Easy to Understand:** The entire system is in one place; you can trace a feature from HTTP request to database in a single file navigation session.
- **Operationally Simple:** One thing to deploy, one thing to monitor, one thing to scale (vertically, at least).

The cost shows up later. As the team grows and the codebase matures, the shared surface area becomes a liability. Every developer touches the same files, the same database context, the same deployment artifact. That coupling that felt like a feature at Day 1 starts to feel like friction at Day 300.

**This lab is designed to make you feel both sides of that tradeoff firsthand.**

---

## What You're Building

**Dom's Garage** is a small automotive shop management system. The scaffold already has:

| Entity | What it models |
|---|---|
| `Car` | Vehicles in the shop |
| `Mechanic` | Staff working the floor |
| `Job` | Open or in-progress work orders |
| `Part` | Inventory items |

What Dom's Garage does not have yet is a **historical record of completed services**; something Dom can pull up and say, *"We did a full engine rebuild on that Charger three months ago."*

You're going to add it: a `ServiceRecord` entity that permanently logs what was done, by whom, on which car, and when.

By the end of this lab you will have:
- Added a new database entity (`ServiceRecord`)
- Extended the shared database context
- Generated and applied a database migration
- Written a business logic service
- Exposed six REST endpoints via a controller
- Verified the feature works end-to-end without breaking anything that already existed

That's a complete feature, front to back. The monolith makes it fast. Pay attention to *how* fast and *what* it costs.

---

## Getting Started

**Project location:** [`labs/1-monolith/lab-start/`](lab-start/DomsGarage.sln)

Open the project in your editor and verify the scaffold is running. From a terminal in the project folder:

```
dotnet run
```

Then navigate to [`http://localhost:5169/swagger`](http://localhost:5169/swagger) (check your terminal for the exact port; it may differ).

**You should see four endpoint groups:** `Cars`, `Jobs`, `Mechanics`, `Parts`. That is your baseline. You'll add a fifth (`ServiceRecords`) by the end of this lab.

> **If the app won't start:** Make sure you're in the `DomsGarage/` folder (the one containing `DomsGarage.csproj`), not the solution root. Run `dotnet restore` if dependencies are missing.

---

## Step 1 — Create the `ServiceRecord` Entity

### What You're Doing

Every feature in a monolith starts with a model. The model describes your data: its shape, its properties, its relationships to other entities. In Entity Framework Core, the model is also what drives your database schema.

The `ServiceRecord` entity has six properties:

| Property | Type | Purpose |
|---|---|---|
| `Id` | `int` | Primary key — auto-assigned by the database |
| `CarId` | `int` | Foreign key → which car was serviced |
| `MechanicId` | `int` | Foreign key → who performed the service |
| `ServiceDescription` | `string` | What was done ("Oil change, filter replaced") |
| `DateCompleted` | `DateTime` | When the service happened |
| `Notes` | `string?` | Optional additional notes (nullable) |

Plus two **navigation properties** (references to the related `Car` and `Mechanic` objects) that EF Core populates when you query service records with `.Include()`.

### Step 1a: Create `ServiceRecord.cs`

**Where:** `Models/` folder  
**File to create:** `ServiceRecord.cs`

Before you write it, take 30 seconds to open `Models/Car.cs` or `Models/Job.cs`. Notice the structure:
- `namespace DomsGarage.Models;`
- `public class [Name]`
- `int Id` primary key
- Foreign key `int` properties
- Navigation properties at the bottom

Your `ServiceRecord` follows the exact same shape. Create `Models/ServiceRecord.cs`:

```csharp
namespace DomsGarage.Models;

/// <summary>
/// A permanent log of a completed service event at Dom's Garage.
/// Links a Car to the Mechanic who serviced it, with a description and date.
/// </summary>
public class ServiceRecord
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public int MechanicId { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public DateTime DateCompleted { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Navigation properties — EF Core populates these when you use .Include()
    public Car? Car { get; set; }
    public Mechanic? Mechanic { get; set; }
}
```

**What each part means:**

| Code Part                        | Description                                                  |
| -------------------------------- | ------------------------------------------------------------ |
| `public int Id { get; set; }`    | EF Core recognizes `Id` by convention as the primary key. No attribute needed. |
| `public int CarId { get; set; }` | Foreign key. EF Core matches `CarId` → `Car.Id` by naming convention. |
| `= string.Empty`                 | Initializes to an empty string, preventing a compiler warning about unset non-nullable strings. |
| `= DateTime.UtcNow`              | Server-side default: if you don't supply a date, it uses "right now." The service layer will also enforce this on creation. |
| `string? Notes`                  | The `?` makes this field optional. EF Core maps it to a nullable column in the database. |
| `Car? Mechanic?`                 | Nullable navigation properties. EF Core populates them at runtime when you use `.Include()`. |

### Step 1b: Uncomment Navigation Properties on Existing Entities

`ServiceRecord` relates to both `Car` and `Mechanic`. EF Core needs both sides of the relationship to be navigable. The scaffold has placeholder comments waiting for you.

**In `Models/Car.cs`**, find this comment near the bottom:

```csharp
// LAB Step 1: Add navigation property for ServiceRecords here
// public ICollection<ServiceRecord> ServiceRecords { get; set; } = [];
```

Uncomment the second line (remove the `//`):

```csharp
// Navigation properties for EF Core
public ICollection<Job> Jobs { get; set; } = [];
public ICollection<ServiceRecord> ServiceRecords { get; set; } = [];
```

**In `Models/Mechanic.cs`**, find and uncomment the same comment:

```csharp
// Navigation properties for EF Core
public ICollection<Job> Jobs { get; set; } = [];
public ICollection<ServiceRecord> ServiceRecords { get; set; } = [];
```

**What `ICollection<ServiceRecord>` means:** This says "a Car has zero or more `ServiceRecords`." EF Core uses this to build the one-to-many relationship. `= []` is C# 12 shorthand for `= new List<ServiceRecord>()`; it initializes the collection to empty so you never get a null reference when accessing it before data loads.

### Verify: Build the Project

```
dotnet build
```

Expected output (Visual Studio):

```shell
Build started at 10:30 PM...
1>------ Build started: Project: DomsGarage, Configuration: Debug Any CPU ------
========== Build: 1 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========
========== Build completed at 10:30 PM and took 03.167 seconds ==========
```

Expected output (Terminal):

```shell
Restore complete (0.4s)
  DomsGarage net10.0 succeeded (0.4s) → bin\Debug\net10.0\DomsGarage.dll

Build succeeded in 1.2s
```

**If you see a compile error mentioning `ServiceRecord` not found** in `Car.cs` or `Mechanic.cs`: verify your new file is named exactly `ServiceRecord.cs` and lives in the `Models` folder with `namespace DomsGarage.Models;` at the top.

### Architecture Moment

You added a new entity to a single flat project. You touched **three files**; all in one folder, all in one namespace. You didn't need to update a contract project, define a repository interface, notify another service, or configure a separate schema layer.

That's the monolith's extension model. Fast. Frictionless. Predictable.

*Project forward: ten developers each adding a feature this week. They're all touching files in this same flat folder. What does a merge conflict look like on Day 300?*

---

## Step 2 — Register in `GarageDbContext` and Run the Migration

### What You're Doing

You have a model. Now you need to do two things:

1. **Tell EF Core that `ServiceRecord` is part of the database** by adding it to `GarageDbContext`
2. **Generate and apply a database migration** so the `ServiceRecords` table is actually created in the SQLite database

This is the most architecturally significant step of the lab. The single line you add to `GarageDbContext` is a physical manifestation of the monolith's central coupling point; everything connects through one shared context.

### Step 2a: Add `DbSet<ServiceRecord>` to `GarageDbContext`

**File:** `Data/GarageDbContext.cs`

Open `GarageDbContext.cs` and find the `DbSet` properties:

```csharp
public DbSet<Car> Cars => Set<Car>();
public DbSet<Mechanic> Mechanics => Set<Mechanic>();
public DbSet<Job> Jobs => Set<Job>();
public DbSet<Part> Parts => Set<Part>();

// LAB Step 2: Add DbSet<ServiceRecord> here
// public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();
```

Uncomment the `ServiceRecords` line:

```csharp
public DbSet<Car> Cars => Set<Car>();
public DbSet<Mechanic> Mechanics => Set<Mechanic>();
public DbSet<Job> Jobs => Set<Job>();
public DbSet<Part> Parts => Set<Part>();
public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();
```

**What this syntax means:** `DbSet<T>` is EF Core's representation of a database table. `Set<T>()` is the base class helper that does the actual work. The expression-bodied property `=> Set<ServiceRecord>()` is equivalent to a `get { return Set<ServiceRecord>(); }` accessor.

### Step 2b: Add `ServiceRecord` Configuration to `OnModelCreating`

Still in `GarageDbContext.cs`, scroll to the `OnModelCreating` method. You'll see configuration blocks for `Car`, `Mechanic`, `Job`, and `Part`. Add the `ServiceRecord` configuration block at the end, before the closing `}` of `OnModelCreating`:

```csharp
modelBuilder.Entity<ServiceRecord>(entity =>
{
    entity.HasKey(sr => sr.Id);
    entity.Property(sr => sr.ServiceDescription).IsRequired().HasMaxLength(500);
    entity.Property(sr => sr.Notes).HasMaxLength(1000);
    entity.HasOne(sr => sr.Car)
          .WithMany(c => c.ServiceRecords)
          .HasForeignKey(sr => sr.CarId);
    entity.HasOne(sr => sr.Mechanic)
          .WithMany(m => m.ServiceRecords)
          .HasForeignKey(sr => sr.MechanicId);
});
```

**What each line does:**

| Line                                                 | What's Going On                                              |
| ---------------------------------------------------- | ------------------------------------------------------------ |
| `entity.HasKey(sr => sr.Id)`                         | Declares `Id` as the primary key (EF Core would infer this, but explicit is clear) |
| `.IsRequired().HasMaxLength(500)`                    | Marks `ServiceDescription` as required with a 500-character cap |
| `entity.Property(sr => sr.Notes).HasMaxLength(1000)` | Configures the optional `Notes` column; no `.IsRequired()` because `Notes` is nullable |
| `HasOne(...).WithMany(...).HasForeignKey(...)`       | Configures the one-to-many relationships for both Car and Mechanic |

### Step 2c: Generate the EF Core Migration

Open a terminal in the `DomsGarage/` project folder and run:

```
dotnet ef migrations add AddServiceRecord
```

**Expected output:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

**What just happened:** EF Core compared your current model (now including `ServiceRecord`) to the last migration snapshot and generated a new migration file describing the change; specifically, "add a `ServiceRecords` table with these columns and foreign keys." You'll see two new files appear in `Migrations/`.

> **If you get:** `'dotnet-ef' was not found`, run `dotnet tool install --global dotnet-ef` first, then retry.
>
> **If you get:** `An operation was already scaffolded for this context`, a pending migration already exists. Run `dotnet ef migrations list` to check. If `AddServiceRecord` is already there, skip to Step 2d.

### Step 2d: Apply the Migration

```
dotnet ef database update
```

**Expected output:**
```
Build started...
Build succeeded.
Applying migration '20260503xxxxxx_AddServiceRecord'.
Done.
```

The `ServiceRecords` table now exists in `garage.db`.

> **Note:** `Program.cs` calls `GarageSeeder.SeedAsync(db)` on startup, which internally calls `db.Database.MigrateAsync()`. This means pending migrations are also applied automatically when you start the app. Running `dotnet ef database update` manually gives you immediate feedback if something is wrong.

### Verify: Build Again

```
dotnet build
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

### Architecture Moment: The Omniscient DbContext

You just added one line to `GarageDbContext`. That file now knows about five entities: Cars, Mechanics, Jobs, Parts, and ServiceRecords. At five entities, this file is still readable. At forty (realistic for a maturing product)  `GarageDbContext` becomes a **change-collision hazard**. Every developer adding a feature touches this one file. That's architectural debt compounding quietly.

*Ten developers, ten features, ten weeks; forty lines added to one file by forty different people. That's a merge conflict waiting to happen.*

---

## Step 3 — Create `ServiceRecordService`

### What You're Doing

`ServiceRecordService` is the business logic layer for service records. It handles all database operations: reading, creating, updating, and deleting. The controller (Step 4) will call into this service; it doesn't touch the database directly.

This is the monolith's service pattern:
- One service per feature area
- Receives `GarageDbContext` directly via constructor injection
- Contains all operations for that feature in one place
- Async all the way down

### Pattern Review: Read `JobService` First

Before writing your service, open `Services/JobService.cs` for two minutes. The shape you're following:

**1. Primary constructor injection:**
```csharp
public class JobService(GarageDbContext db)
```
C# primary constructor syntax means that the `db` is available throughout the class with no field declaration needed.

**2. Async queries with `.Include()`:**
```csharp
public async Task<List<Job>> GetAllAsync() =>
    await db.Jobs.Include(j => j.Car).Include(j => j.Mechanic).ToListAsync();
```
`.Include()` tells EF Core to load related entities in the same query. Without it, navigation properties are `null`.

**3. Nullable return for `GetByIdAsync`:**
```csharp
public async Task<Job?> GetByIdAsync(int id) =>
    await db.Jobs.FirstOrDefaultAsync(j => j.Id == id);
```
Returns `null` when not found; the controller turns that into a 404.

Your `ServiceRecordService` follows every one of these patterns.

### Step 3a: Create `ServiceRecordService.cs`

**Where:** `Services/` folder  
**File to create:** `ServiceRecordService.cs`

```csharp
using DomsGarage.Data;
using DomsGarage.Models;
using Microsoft.EntityFrameworkCore;

namespace DomsGarage.Services;

/// <summary>
/// Manages service record logging for Dom's Garage.
/// Handles CRUD for ServiceRecord — follows the same pattern as JobService and CarService.
/// </summary>
public class ServiceRecordService(GarageDbContext db)
{
    public async Task<List<ServiceRecord>> GetAllAsync() =>
        await db.ServiceRecords
                .Include(sr => sr.Car)
                .Include(sr => sr.Mechanic)
                .ToListAsync();

    public async Task<ServiceRecord?> GetByIdAsync(int id) =>
        await db.ServiceRecords
                .Include(sr => sr.Car)
                .Include(sr => sr.Mechanic)
                .FirstOrDefaultAsync(sr => sr.Id == id);

    public async Task<List<ServiceRecord>> GetByCarIdAsync(int carId) =>
        await db.ServiceRecords
                .Where(sr => sr.CarId == carId)
                .Include(sr => sr.Mechanic)
                .ToListAsync();

    public async Task<ServiceRecord> CreateAsync(ServiceRecord record)
    {
        db.ServiceRecords.Add(record);
        await db.SaveChangesAsync();
        return record;
    }

    public async Task<ServiceRecord?> UpdateAsync(int id, ServiceRecord updated)
    {
        ServiceRecord? existing = await db.ServiceRecords.FindAsync(id);
        if (existing is null) return null;

        existing.CarId = updated.CarId;
        existing.MechanicId = updated.MechanicId;
        existing.ServiceDescription = updated.ServiceDescription;
        existing.DateCompleted = updated.DateCompleted;
        existing.Notes = updated.Notes;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        ServiceRecord? record = await db.ServiceRecords.FindAsync(id);
        if (record is null) return false;

        db.ServiceRecords.Remove(record);
        await db.SaveChangesAsync();
        return true;
    }
}
```

**Method-by-method explanation:**

| Method                                           | Explanation                                                  |
| ------------------------------------------------ | ------------------------------------------------------------ |
| **`GetAllAsync()`**                              | Loads every service record with related `Car` and `Mechanic` populated. The `.Include()` calls generate a JOIN in SQL so `record.Car.Make` is already available in the response. |
| **`GetByIdAsync(int id)`**                       | Single record by primary key, with navigation properties. Returns `null` if not found. |
| **`GetByCarIdAsync(int carId)`**                 | Filtered query for a specific car's service history. Only includes `Mechanic` (not `Car`) because we already know which car we're asking about. |
| **`CreateAsync(ServiceRecord record)`**          | Adds the record, saves, and returns it with the database-assigned `Id` populated. After `SaveChangesAsync()`, EF Core updates `record.Id` with the value the database assigned. |
| **`UpdateAsync(int id, ServiceRecord updated)`** | Load-and-update pattern: finds the existing record, copies fields from the submitted object, saves. EF Core change tracking generates a targeted `UPDATE` statement for only the modified columns. |
| **`DeleteAsync(int id)`**                        | Finds by identifier, removes, saves. Returns `true` if deleted, `false` if not found. |

### Step 3b: Register `ServiceRecordService` in `Program.cs`

Open `Program.cs` and find:

```csharp
// LAB Step 3: Register ServiceRecordService here
// builder.Services.AddScoped<ServiceRecordService>();
```

Uncomment that line:

```csharp
builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<MechanicService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<PartService>();
builder.Services.AddScoped<ServiceRecordService>();
```

**What `AddScoped` means:** One instance of `ServiceRecordService` per HTTP request. This matches the lifetime of `GarageDbContext` (also scoped), which is critical because the service holds a reference to `db`. Mismatched lifetimes cause runtime errors.

### Verify: Build the Project

```
dotnet build
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

**If you see:** `The name 'ServiceRecords' does not exist`, verify that it was added to `GarageDbContext` in Step 2.

### Architecture Moment: No Interface Here — Intentionally

Notice there is no `IServiceRecordService` interface. In the scaffold, every service is the same (concrete class, no wrapper. This is intentional for the monolith. An interface adds a file, a contract, and navigation overhead) but at this scale, it buys nothing. There's only one implementation.

*Module 2 will introduce `ICustomerRepository` for a specific reason: to enforce the boundary that makes N-Tier different. You'll feel the difference when you get there.*

---

## Step 4 — Create `ServiceRecordsController`

### What You're Doing

`ServiceRecordsController` exposes your `ServiceRecordService` as HTTP REST endpoints. This is the outermost layer, it receives HTTP requests, calls the service, and returns HTTP responses. It contains no business logic and does no database work directly.

By the end of this step, you'll have six REST endpoints available in Swagger:

| Method | Endpoint | What it does |
|---|---|---|
| `GET` | `/api/servicerecords` | List all service records |
| `GET` | `/api/servicerecords/{id}` | Get one service record by ID |
| `GET` | `/api/servicerecords/car/{carId}` | Get all records for a specific car |
| `POST` | `/api/servicerecords` | Log a new service record |
| `PUT` | `/api/servicerecords/{id}` | Update an existing record |
| `DELETE` | `/api/servicerecords/{id}` | Remove a record |

### Pattern Review: Read `JobsController` First

Open `Controllers/JobsController.cs` briefly. The shape you want:

```csharp
[ApiController]
[Route("api/jobs")]
public class JobsController(JobService jobService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Job>>> GetAll() =>
        Ok(await jobService.GetAllAsync());
    // ...
}
```

Key patterns:
| Pattern                                                      | Description                                                  |
| ------------------------------------------------------------ | ------------------------------------------------------------ |
| `[ApiController]`                                            | Enables automatic model validation and automatic 400 responses for bad requests. |
| `[Route("api/jobs")]`                                        | Defines the base URL for all methods in this controller.     |
| Primary constructor injection                                | The service is provided by ASP.NET Core's DI container.      |
| `ControllerBase` (not `Controller`)                          | API controllers don't need view support.                     |
| `ActionResult<T>` return types                               | Lets you return both typed data and HTTP status codes.       |
| `Ok(...)`, `NotFound()`, `CreatedAtAction(...)`, `NoContent()` | Helper methods for correct HTTP status codes.                |

### Create `ServiceRecordsController.cs`

**Where:** `Controllers/` folder  
**File to create:** `ServiceRecordsController.cs`

```csharp
using DomsGarage.Models;
using DomsGarage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DomsGarage.Controllers;

/// <summary>
/// REST endpoints for service record logging.
/// Exposes ServiceRecordService as HTTP endpoints via ASP.NET Core routing.
/// </summary>
[ApiController]
[Route("api/servicerecords")]
public class ServiceRecordsController(ServiceRecordService serviceRecordService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ServiceRecord>>> GetAll() =>
        Ok(await serviceRecordService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceRecord>> GetById(int id)
    {
        ServiceRecord? record = await serviceRecordService.GetByIdAsync(id);
        return record is null ? NotFound() : Ok(record);
    }

    [HttpGet("car/{carId:int}")]
    public async Task<ActionResult<List<ServiceRecord>>> GetByCar(int carId) =>
        Ok(await serviceRecordService.GetByCarIdAsync(carId));

    [HttpPost]
    public async Task<ActionResult<ServiceRecord>> Create(ServiceRecord record)
    {
        ServiceRecord created = await serviceRecordService.CreateAsync(record);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceRecord>> Update(int id, ServiceRecord record)
    {
        ServiceRecord? updated = await serviceRecordService.UpdateAsync(id, record);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await serviceRecordService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
```

**Endpoint notes:**

| Endpoint                                  | Note                                                         |
| ----------------------------------------- | ------------------------------------------------------------ |
| **`GET /api/servicerecords`**             | Returns 200 OK with the full list.                           |
| **`GET /api/servicerecords/{id}`**        | Returns 200 OK with one record, or **404 Not Found** if the ID doesn't exist. The `{id:int}` route constraint means ASP.NET Core rejects non-integer values before your code runs. |
| **`GET /api/servicerecords/car/{carId}`** | Returns all service records for a car. Returns an empty array `[]` (not 404) if the car has no records yet. |
| **`POST /api/servicerecords`**            | Creates a record, returns **201 Created** (not 200 OK). The 201 status is the HTTP standard for "resource created" and includes a `Location` header pointing to where you can `GET` the new resource. |
| **`PUT /api/servicerecords/{id}`**        | Updates a record. Returns 200 OK with the updated record, or 404 if not found. |
| **`DELETE /api/servicerecords/{id}`**     | Returns **204 No Content** on success (HTTP standard for successful deletion with no body) or 404 if not found. |

**Request body for POST:**

```json
{
  "carId": 1,
  "mechanicId": 1,
  "serviceDescription": "Oil change and filter replacement",
  "notes": "Customer requested synthetic oil"
}
```

Do not include `Id` (assigned by the database) or `DateCompleted` (set by the service). The `notes` field is optional.

### Verify: Build the Project

ASP.NET Core discovers controllers automatically by scanning for classes that inherit from `ControllerBase` and have `[ApiController]`. You don't need to register the controller anywhere `app.MapControllers()` in `Program.cs` handles it.

```
dotnet build
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

### Architecture Moment: Feature Coupling at Scale

Step back and count the files you've touched since Step 1:

1. `Models/ServiceRecord.cs` — created
2. `Models/Car.cs` — navigation property added
3. `Models/Mechanic.cs` — navigation property added
4. `Data/GarageDbContext.cs` — DbSet + configuration added
5. `Services/ServiceRecordService.cs` — created
6. `Program.cs` — service registration added
7. `Controllers/ServiceRecordsController.cs` — created

**Seven files. One feature. All in a single flat project.**

*Now multiply by ten. Ten features, seventy files, one flat folder, no module boundaries. What does onboarding look like? What does code review look like? What does blame look like when something breaks? Module 2 starts to address that question.*

---

## Step 5 — Smoke Test Your Implementation

### What You're Doing

A **smoke test** answers one question: *Does the whole thing work?* It's not exhaustive testing; it's the confidence check. You run the application, exercise the key paths, and confirm the endpoints respond correctly.

This step also tests the monolith's shared failure domain: because everything shares `GarageDbContext`, a mistake in your new code can affect the existing endpoints. The smoke test catches that.

### Start the Application

From the `DomsGarage/` project folder:

```
dotnet run
```

Watch the startup output. You should see something like:

```
Building...
Now listening on: http://localhost:5169
```

> **If you see an EF Core error** like `no such table: ServiceRecords`, run `dotnet ef database update` (Step 2d), then restart.
>
> **If the app won't start with** `No service registered for type 'ServiceRecordService'`, verify `AddScoped<ServiceRecordService>()` is uncommented in `Program.cs`.

### Open Swagger UI

Navigate to [[``http://localhost:5169/swagger``]](http://localhost:5169/swagger) (use your actual port from the terminal).

**You should see:** Five endpoint groups: Cars, Jobs, Mechanics, Parts, and now **ServiceRecords**. Expand ServiceRecords to confirm all six endpoints are listed.

If ServiceRecords doesn't appear, verify `ServiceRecordsController.cs` has both `[ApiController]` and `[Route("api/servicerecords")]` attributes.

### Test 1: Create a Service Record (POST)

Click `POST /api/servicerecords` → **"Try it out"** → edit the request body:

```json
{
  "carId": 1,
  "mechanicId": 1,
  "serviceDescription": "Full engine rebuild — 900hp build for the quarter mile"
}
```

> **Why CarId 1 and MechanicId 1?** The seed data creates a 1970 Dodge Charger (Car ID 1) with Dominic Toretto as the mechanic (Mechanic ID 1). These identifiers exist in your database, satisfying the foreign key constraint.

Click **"Execute"**.

**Expected:** HTTP **201 Created** with the new record in the response body. Note that `id` and `dateCompleted` were assigned by the service/database — you didn't supply them.

> **If you get 500 "FOREIGN KEY constraint failed":** Use `carId: 1` and `mechanicId: 1`, these are the IDs created by seed data.
>
> **If you get 400 Bad Request:** Check your JSON for missing quotes, trailing commas, or mismatched braces.

### Test 2: Get All Service Records (GET)

Click `GET /api/servicerecords` → **"Try it out"** → **"Execute"**.

**Expected:** 200 OK with an array containing the record you just created, with `car` and `mechanic` objects fully populated.

### Test 3: Get a Single Record (GET by ID)

Click `GET /api/servicerecords/{id}` → **"Try it out"** → enter `1` → **"Execute"**.

**Expected:** 200 OK with the single record.

Now enter `999` → **"Execute"**.

**Expected:** **404 Not Found** — confirms your null-check is working correctly.

### Test 4: Get Records by Car (GET by Car ID)

Click `GET /api/servicerecords/car/{carId}` → **"Try it out"** → enter `1` → **"Execute"**.

**Expected:** 200 OK with an array containing the one record for Car ID 1.

Try Car ID `2`:

**Expected:** 200 OK with an **empty array** `[]` — not 404. An empty list is a valid result.

### Test 5: Regression Check — Existing Endpoints Still Work

This is the most important test: *did your changes break anything that was already working?*

Test each quickly:

- `GET /api/cars` → should return the two seed cars (Dodge Charger, Toyota Supra)
- `GET /api/mechanics` → should return Dominic Toretto and Han Seoul-Oh
- `GET /api/jobs` → should return the seed job (engine rebuild on the Charger)
- `GET /api/parts` → should return the NOS canister

All four should return **200 OK** with data.

**If any existing endpoint returns 500:** Check `GarageDbContext.cs` for syntax errors in the `OnModelCreating` additions. A misconfigured `Entity<T>()` block can break migrations and queries for other entities.

**If existing endpoints return empty results:** The database may have been reset. Delete `garage.db` and restart; `GarageSeeder.SeedAsync()` will recreate and reseed it on startup.

### Optional: Test 6 — Add a Second Record

```json
{
  "carId": 2,
  "mechanicId": 2,
  "serviceDescription": "Suspension tuning and alignment check"
}
```

Then call `GET /api/servicerecords,` and you should see both records. Call `GET /api/servicerecords/car/2,` and you should see just the second one. This confirms the filtering logic works correctly.

### Architecture Moment: Shared Failure Domain

You ran the regression check, and everything passed. Now think about what would have happened if it *hadn't*.

If your change to `GarageDbContext.OnModelCreating` had a syntax error, `GET /api/cars` would have broken even though you never touched `CarsController` or `CarService`. Because everything runs through the same context, and the context is shared. You can break anything by touching the central coupling point, even accidentally.

*One migration. One context. Everything ships together. Everything breaks together if it breaks. That's the tradeoff you've just experienced firsthand.*

---

## Completion Checklist

Work through this list to confirm your implementation is complete before calling it done.

### Step 1 — Entity
- [ ] `Models/ServiceRecord.cs` created with all six properties (`Id`, `CarId`, `MechanicId`, `ServiceDescription`, `DateCompleted`, `Notes`)
- [ ] Navigation properties in `Models/Car.cs` uncommented: `public ICollection<ServiceRecord> ServiceRecords { get; set; } = [];`
- [ ] Navigation properties in `Models/Mechanic.cs` uncommented: `public ICollection<ServiceRecord> ServiceRecords { get; set; } = [];`

### Step 2 — DbContext & Migration
- [ ] `Data/GarageDbContext.cs`: `public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();` added
- [ ] `Data/GarageDbContext.cs`: `ServiceRecord` configuration block added to `OnModelCreating`
- [ ] Migration file `...AddServiceRecord.cs` present in `Migrations/`
- [ ] Migration applied: `dotnet ef database update` succeeded (or app started and auto-migrated)

### Step 3 — Service
- [ ] `Services/ServiceRecordService.cs` created with all six methods: `GetAllAsync`, `GetByIdAsync`, `GetByCarIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- [ ] `Program.cs`: `builder.Services.AddScoped<ServiceRecordService>();` uncommented

### Step 4 — Controller
- [ ] `Controllers/ServiceRecordsController.cs` created with all six endpoints (GET all, GET by ID, GET by car, POST, PUT, DELETE)

### Step 5 — Smoke Test
- [ ] `POST /api/servicerecords` → 201 Created ✅
- [ ] `GET /api/servicerecords` → 200 OK with data ✅
- [ ] `GET /api/servicerecords/{id}` → 200 OK; 404 for unknown ID ✅
- [ ] `GET /api/servicerecords/car/{carId}` → 200 OK, filtered correctly ✅
- [ ] `GET /api/cars` → 200 OK (regression) ✅
- [ ] `GET /api/mechanics` → 200 OK (regression) ✅
- [ ] `GET /api/jobs` → 200 OK (regression) ✅
- [ ] `GET /api/parts` → 200 OK (regression) ✅

---

## Troubleshooting

| Symptom | Likely Cause | Fix |
|---|---|---|
| App won't start: `ServiceRecordService not found` | `AddScoped<ServiceRecordService>()` not uncommented | Uncomment in `Program.cs`, rebuild |
| App won't start: EF Core migration error | `DbSet` added but no migration run | Run `dotnet ef migrations add AddServiceRecord`, then `dotnet ef database update` |
| POST returns 500: FOREIGN KEY constraint | `carId` or `mechanicId` doesn't exist in DB | Use `1` for both — these are seed data IDs |
| POST returns 400 | Malformed JSON | Check for missing quotes, trailing commas, mismatched braces |
| GET returns 404 unexpectedly | Requesting an ID that doesn't exist | Confirm the record was created; use the `id` from the POST response |
| Existing endpoints return 500 | Error in `GarageDbContext.OnModelCreating` | Compare your `ServiceRecord` config block against the `Job` config block for syntax |
| ServiceRecords missing from Swagger | Controller missing `[ApiController]` or `[Route]` attribute, or wrong folder | Verify attributes are present at the class level |
| `dotnet-ef` not found | EF Core CLI tools not installed | Run `dotnet tool install --global dotnet-ef`, restart terminal, retry |
| `ServiceRecord` not found in Car.cs | New file missing namespace or wrong folder | Verify `namespace DomsGarage.Models;` is at the top of `ServiceRecord.cs` in `Models/` |
| Build error: navigation property missing | `Car.cs` or `Mechanic.cs` navigation property still commented out | Remove the `//` from the `ICollection<ServiceRecord>` lines |

---

## What You Just Demonstrated

If all smoke tests passed, you have successfully:

- ✅ Added a new entity to a running .NET application
- ✅ Extended the shared database context
- ✅ Generated and applied a database migration
- ✅ Written a service layer following the established pattern
- ✅ Exposed six REST endpoints via a controller
- ✅ Verified the feature works end-to-end via Swagger
- ✅ Confirmed no regressions to existing functionality

**That's a complete feature, front to back, in approximately 30 minutes. That feeling is the lesson.**

The monolith made it fast. Seven files, one project, one namespace, no walls — and it worked. Now carry that experience into the debrief: *what made it fast, and what would slow it down at scale?* That question is what the rest of the day is built to answer.
