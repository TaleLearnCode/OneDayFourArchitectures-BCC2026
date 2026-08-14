# Toretto Motors — N-Tier Scaffold (Lab-start)

**Module:** 2 — N-Tier Architecture  
**Status:** Lab-start scaffold (participants will extend during lab)

## Project Structure

Three separate projects enforcing N-Tier architecture boundaries:

### 1. TorettoMotors.DAL (Data Access Layer)
- **Entities:** Customer, Vehicle, Part, Invoice, MaintenancePlan
- **Repositories:** CRUD operations via EF Core
- **DbContext:** SQLite database configuration

### 2. TorettoMotors.BLL (Business Logic Layer)
- **DTOs:** Model transformation layer
- **Services:** Business logic and validation
- **Interfaces:** Decoupled from concrete implementations

### 3. TorettoMotors.Api (ASP.NET Core Web API)
- **Controllers:** REST endpoints for all entities
- **Program.cs:** Dependency injection composition root
- **Swagger:** API documentation and testing

## Architecture Principles

✓ **Enforced Dependency Direction:** Api → BLL → DAL → (database)  
✓ **No DAL References in API:** Controllers only see BLL services  
✓ **Interface-based DI:** All layer communication via contracts  
✓ **Single Composition Root:** Program.cs wires all dependencies  

## Running the Application

```bash
cd TorettoMotors.Api
dotnet run
```

Access Swagger at: `https://localhost:5001/swagger`

## Database

SQLite database (`toretto.db`) is created automatically on first run via EF Core migrations.

## Entities (4 Pre-built + 1 Stub)

### Pre-built (Fully Implemented)
- **Customer** - CRUD complete
- **Vehicle** - CRUD + customer filtering
- **Part** - CRUD complete
- **Invoice** - CRUD + customer filtering + validation

### Stub (For Lab)
- **MaintenancePlan** - Entity and service interfaces defined; endpoints respond
  - Participants implement business logic during lab

## Lab Feature

**Maintenance Plan:** Subscription-based preventive maintenance service

Participants will add:
- Business logic validation
- Additional filtering/search
- Advanced features (e.g., renewal logic)

---

**Built:** 2026-05-05  
**For:** One Day, Four Architectures — Module 2 Workshop
