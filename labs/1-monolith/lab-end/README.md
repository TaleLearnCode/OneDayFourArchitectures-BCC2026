# Dom's Garage — Module 1: Monolith

A single-location auto repair shop management system. Built as the Module 1 demo for the *One Day, Four Architectures* workshop.

## What This Demonstrates

This application is a **monolith**: one project, one deployed artifact, one shared database. Everything ships together.

Key architectural characteristics to explore:
- **Single `.csproj`** — one `dotnet publish` produces one output
- **Flat folder structure** — `/Models`, `/Services`, `/Controllers`, `/Data` all at the same level
- **Shared `GarageDbContext`** — one context knows about every entity
- **No network hops** — services call each other as direct method invocations
- **No layer enforcement** — any service can reference any other service

## Getting Started

```bash
# From this directory
dotnet run
```

Then open **http://localhost:5050/swagger** to explore the API.

The SQLite database (`garage.db`) is created and seeded automatically on first run.

## Project Structure

```
DomsGarage/
├── Controllers/     # REST endpoints (one per entity)
├── Data/            # GarageDbContext + seeder
├── Migrations/      # EF Core migrations (pre-generated)
├── Models/          # Entity classes + enums
├── Services/        # Business logic (direct DbContext injection)
└── Program.cs       # DI wiring — readable in 30 seconds
```

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/cars` | List all cars |
| GET | `/api/cars/{id}` | Get car by ID |
| POST | `/api/cars` | Check in a car |
| PUT | `/api/cars/{id}` | Update car details |
| DELETE | `/api/cars/{id}` | Remove a car |
| **PATCH** | **`/api/cars/{id}/ready`** | **Flag car ready for pickup (key business rule)** |
| GET | `/api/jobs` | List all jobs |
| GET | `/api/jobs/{id}` | Get job by ID |
| GET | `/api/jobs/car/{carId}` | Get jobs for a car |
| POST | `/api/jobs` | Open a new job |
| PATCH | `/api/jobs/{id}/close` | Close a job (auto-updates car status) |
| DELETE | `/api/jobs/{id}` | Delete a job |
| GET | `/api/mechanics` | List all mechanics |
| GET | `/api/mechanics/{id}` | Get mechanic by ID |
| POST | `/api/mechanics` | Add a mechanic |
| PUT | `/api/mechanics/{id}` | Update mechanic details |
| DELETE | `/api/mechanics/{id}` | Remove a mechanic |
| GET | `/api/parts` | List parts inventory |
| GET | `/api/parts/{id}` | Get part by ID |
| POST | `/api/parts` | Add a part |
| PUT | `/api/parts/{id}` | Update part details |
| DELETE | `/api/parts/{id}` | Remove a part |
| PATCH | `/api/parts/{id}/stock?delta={n}` | Adjust stock level |

## The Key Business Rule

`PATCH /api/cars/{id}/ready` → `CarService.FlagReadyForPickupAsync()`

This is the walkthrough moment. The method:
1. Loads the car with all its jobs (one DbContext, one query)
2. Validates all jobs are closed
3. Sets `Car.Status = ReadyForPickup`

No HTTP calls. No queues. No retries. One method, one database, instant response. **That's the monolith pattern.**

## Lab: Adding Service Record Logging

The `ServiceRecord` entity is intentionally absent — participants add it in the lab.

Look for `// LAB Step N:` comments throughout the code to find the exact extension points.

## Tech Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core 10 (SQLite)
- Swashbuckle / Swagger UI
