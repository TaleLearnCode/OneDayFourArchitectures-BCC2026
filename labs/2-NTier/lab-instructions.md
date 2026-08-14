# N-Tier Architecture Lab — Toretto Motors Maintenance Plan Feature

## Introduction: What Is N-Tier, and Why Does It Matter?

After [Module 1](../1-monolith/README.md), you've experienced the monolith's speed and simplicity. Everything in one project. Everything flat. For a small garage and a small team, that's perfect.

**Toretto Motors is a different story.** It's a growing dealership chain with three teams:

- **UI team** owns the customer-facing website and dashboard
- **Backend team** owns business logic: pricing, subscriptions, plan renewal
- **Database team** owns schema migrations and data integrity

If they all edit one flat project, every merge becomes a conflict zone. Business rule changes touch the same files as UI tweaks. Schema updates block UI work.

**That's where N-Tier comes in.** N-Tier doesn't ask developers to play nice; it makes playing nice structurally necessary. The architecture is enforced by the compiler, not culture.

Here's what you'll experience:

- **Visibility:** Three projects in the solution explorer. Three separate `.csproj` files. The structure is physical, not cosmetic.
- **Enforcement:** The API project cannot reference the DAL. Try it, and the build fails. No code review needed. No discipline required. The compiler says no.
- **Contracts:** Services depend on interfaces, not concrete classes. This means the UI team can run tests against a mock backend. The backend team can refactor how data is stored without recompiling the UI.
- **Cost:** Adding business logic now requires touching multiple projects and multiple files. One feature, two interfaces, two implementations. That coordination overhead is the tradeoff.

**This lab is designed to make you feel the strength of N-Tier's boundaries and the cost of maintaining them.**

---

## What You're Building

**Toretto Motors** is a dealership management system. The scaffold includes:

| Entity | What it models | Status |
|---|---|---|
| `Customer` | Dealership customers | ✓ Complete |
| `Vehicle` | Inventory of vehicles for sale | ✓ Complete |
| `Part` | Spare parts for maintenance | ✓ Complete |
| `Invoice` | Sales and service invoicing | ✓ Complete |
| `MaintenancePlan` | Subscription plans for preventive maintenance | 🚧 Partially scaffolded; you will validate BLL logic and finish API wiring |

The `MaintenancePlan` entity and service interfaces are already in place. In this lab, you'll verify/finalize service behavior and expose it cleanly through the API:

- ⚠️ **Validation must be enforced in BLL:** Ensure invalid plans are rejected by service logic.
- ⚠️ **Active-plan filtering must be available through API:** Customers need an endpoint to fetch active plans.
- ⚠️ **Renewal flow must be callable end-to-end:** Service and controller method signatures must match.

You are going to **enhance the MaintenancePlanService** with validation, filtering, and business logic. Most changes are in the **Business Logic Layer**, with one small but required API update to expose and test the new behavior.  The **DAL remains untouched**.

By the end of this lab, you will have:
- Verified/refined validation logic in `MaintenancePlanService` (ensure `MonthlyPrice > 0`, required fields, valid start date)
- Verified/refined filtering and renewal methods in BLL
- Fixed and exposed thin controller actions to call the service methods correctly
- Verified the feature works end-to-end through Swagger

That's a complete feature enhancement with **business rules in BLL** and a **thin API layer** that simply exposes those rules.

---

## Getting Started

**Project location:** [`labs/2-n-tier/TorettoMotors/`](TorettoMotors/TorettoMotors.sln)

Open the solution in your editor and verify the scaffold is running. From a terminal in the solution root:

```bash
cd labs/2-n-tier/TorettoMotors
dotnet run --project TorettoMotors.Api
```

Then navigate to the Swagger URL shown in your terminal output (commonly [`http://localhost:5000/swagger`](http://localhost:5000/swagger)).

**You should see endpoint groups for:** Customers, Vehicles, Parts, Invoices, and **MaintenancePlans**.  
If the API does not build yet due to a renewal method mismatch, continue with the lab steps and fix the controller action in Step 5b before final end-to-end testing.

> **If the app won't start:** Make sure you're in the `TorettoMotors` folder when opening the solution. Run `dotnet restore` if dependencies are missing. Check that all three projects exist: `TorettoMotors.Api`, `TorettoMotors.BLL`, `TorettoMotors.DAL`.

---

## The Three-Project Structure — What You're Looking At

Before you write code, spend 60 seconds examining the solution. This is the architecture.

**In Solution Explorer or your file system, you should see:**

```
TorettoMotors/
├── TorettoMotors.Api/
│   ├── TorettoMotors.Api.csproj         ← References only TorettoMotors.BLL
│   ├── Controllers/                     ← API endpoints: never sees DbContext or entities
│   ├── Program.cs                       ← Composition root: all DI registrations
│   └── ...
├── TorettoMotors.BLL/
│   ├── TorettoMotors.BLL.csproj         ← References only TorettoMotors.DAL
│   ├── Models/                          ← DTOs: MaintenancePlanDto, CustomerDto, etc.
│   ├── Services/
│   │   ├── Interfaces/                  ← IMaintenancePlanService (contract you'll enhance)
│   │   └── Implementations/             ← MaintenancePlanService (where you add logic)
│   └── ...
└── TorettoMotors.DAL/
    ├── TorettoMotors.DAL.csproj         ← References only EF Core
    ├── Entities/                        ← MaintenancePlanEntity, CustomerEntity, etc.
    ├── Repositories/
    │   ├── Interfaces/                  ← IMaintenancePlanRepository (data access contracts)
    │   └── Implementations/             ← MaintenancePlanRepository (EF Core queries)
    ├── Context/                         ← TorettoDbContext (never seen by API or BLL)
    └── ...
```

**Key principle:** Each project references exactly one other project (downward only). Try adding a reference from `Api` to `DAL` and the solution won't compile. The compiler enforces the boundary.

**Key observations:**

1. **Three `.csproj` files, one per layer.** Each layer is a separate project compiled to a separate assembly.
2. **Dependency chain is one-directional:** Api → BLL → DAL. No circles. No shortcuts. The compiler enforces this by refusing to add a project reference that would create a cycle.
3. **DAL project reference does NOT exist in Api project.** Open `TorettoMotors.Api/TorettoMotors.Api.csproj` and search for `ProjectReference`. You'll see only BLL. This is not cosmetic — it's the architecture. Try to inject `TorettoDbContext` into a controller, and the build fails.
4. **DbContext lives exclusively in DAL.** BLL service classes never reference `TorettoDbContext`. They depend on repository interfaces.
5. **Controllers depend on BLL interfaces, not implementations.** `MaintenancePlansController` constructor parameter is `IMaintenancePlanService`, not `MaintenancePlanService`.

This structure is different from Module 1 (Monolith) in one critical way: **the compiler enforces boundaries**. In the monolith, a developer could technically inject `TorettoDbContext` into a controller and the build would still succeed. Here, it's structurally impossible. That's the upgrade.

---

## Step 1 — Understand the Existing Pattern

Before you enhance the MaintenancePlan service, let's confirm you understand how the three layers work together.

### Step 1a: Read `IMaintenancePlanService` and `MaintenancePlanService`

**File location:** `TorettoMotors.BLL/Services/Interfaces/IMaintenancePlanService.cs` and `TorettoMotors.BLL/Services/Implementations/MaintenancePlanService.cs`

Spend 60 seconds skimming both files. Notice:

- `IMaintenancePlanService` defines CRUD methods with domain-specific names: `GetMaintenancePlanByIdAsync()`, `GetAllMaintenancePlansAsync()`, `CreateMaintenancePlanAsync()`, `UpdateMaintenancePlanAsync()`, `DeleteMaintenancePlanAsync()`.
- `MaintenancePlanService` implements the interface and injects `IMaintenancePlanRepository` (not a concrete class, not the DbContext).
- `MaintenancePlanService` contains CRUD plus business logic methods (validation, active filtering, renewal).

### Step 1b: Read `MaintenancePlanEntity` and `MaintenancePlanDto`

**File locations:** 
- Entity: `TorettoMotors.DAL/Entities/MaintenancePlanEntity.cs`
- DTO: `TorettoMotors.BLL/Models/MaintenancePlanDto.cs`

Notice:

- `MaintenancePlanEntity` is the database model (managed by EF Core in DAL only).
- `MaintenancePlanDto` is the business model (used by the service to exchange data with the API layer).
- Fields include: `Id`, `CustomerId`, `Name`, `Description`, `MonthlyPrice`, `StartDate`, `Status`.

### Step 1c: Examine the `MaintenancePlanRepository`

**File location:** `TorettoMotors.DAL/Repositories/Interfaces/IMaintenancePlanRepository.cs`

Notice the data access contracts:
- `GetByIdAsync(id)` — fetches a plan by ID
- `GetAllAsync()` — fetches all plans
- `GetByCustomerIdAsync(customerId)` — fetches plans for a specific customer
- `AddAsync()`, `UpdateAsync()`, `DeleteAsync()` — CRUD operations

### Step 1d: Open `Program.cs` — The Composition Root

**File location:** `TorettoMotors.Api/Program.cs`

Search for `AddScoped` and find these registrations:

```csharp
builder.Services.AddScoped<IMaintenancePlanRepository, MaintenancePlanRepository>();
builder.Services.AddScoped<IMaintenancePlanService, MaintenancePlanService>();
```

This is the **only place** where concrete implementations are wired to interfaces. BLL and DAL projects don't self-register. The API project is the composition root.

---

## Step 2 — Verify/Refine Validation in `MaintenancePlanService`

### What You're Doing

`MaintenancePlanService` should enforce business validation before persisting. In this scaffold, most validation exists already — verify it and refine if needed.

This teaches two lessons:
1. Business logic belongs in the BLL service, not the controller or database
2. Validation in the service layer keeps the API layer thin

### Step 2a: Verify Shared Validation Is Called

**File:** `TorettoMotors.BLL/Services/Implementations/MaintenancePlanService.cs`

Confirm `CreateMaintenancePlanAsync()` calls shared validation before mapping/saving:

```csharp
public async Task<MaintenancePlanDto> CreateMaintenancePlanAsync(MaintenancePlanDto plan)
{
    ValidateEntity(plan);

    var entity = new MaintenancePlanEntity
    {
        CustomerId = plan.CustomerId,
        Name = plan.Name,
        Description = plan.Description,
        MonthlyPrice = plan.MonthlyPrice,
        StartDate = plan.StartDate,
        Status = plan.Status ?? "Active"
    };

    var created = await _maintenancePlanRepository.AddAsync(entity);
    return MapToDto(created);
}
```

### Step 2b: Verify Update Uses the Same Validation

**File:** `TorettoMotors.BLL/Services/Implementations/MaintenancePlanService.cs`

Confirm `UpdateMaintenancePlanAsync()` uses the same validation logic:

```csharp
public async Task<MaintenancePlanDto> UpdateMaintenancePlanAsync(MaintenancePlanDto plan)
{
    ValidateEntity(plan);

    var entity = new MaintenancePlanEntity
    {
        Id = plan.Id,
        CustomerId = plan.CustomerId,
        Name = plan.Name,
        Description = plan.Description,
        MonthlyPrice = plan.MonthlyPrice,
        StartDate = plan.StartDate,
        Status = plan.Status ?? "Active"
    };

    var updated = await _maintenancePlanRepository.UpdateAsync(entity);
    return MapToDto(updated);
}
```

---

## Step 3 — Verify Filtering Method in the Service Interface (~3 min)

### What You're Doing

Customers often want to retrieve only their **active** maintenance plans. Confirm the interface and implementation support this.

This teaches: **Business logic belongs in BLL first.** Then add thin API actions only when you need to expose and test that behavior externally.

### Step 3a: Extend `IMaintenancePlanService` Interface

**File:** `TorettoMotors.BLL/Services/Interfaces/IMaintenancePlanService.cs`

Confirm this method signature exists in the interface (add it only if missing):

```csharp
/// <summary>
/// Get all active maintenance plans for a customer.
/// </summary>
Task<IEnumerable<MaintenancePlanDto>> GetActiveMaintenancePlansByCustomerIdAsync(int customerId);
```

The full interface should include (in addition to the existing methods):

```csharp
public interface IMaintenancePlanService
{
    Task<MaintenancePlanDto?> GetMaintenancePlanByIdAsync(int id);
    Task<IEnumerable<MaintenancePlanDto>> GetAllMaintenancePlansAsync();
    Task<IEnumerable<MaintenancePlanDto>> GetMaintenancePlansByCustomerIdAsync(int customerId);
    Task<MaintenancePlanDto> CreateMaintenancePlanAsync(MaintenancePlanDto plan);
    Task<MaintenancePlanDto> UpdateMaintenancePlanAsync(MaintenancePlanDto plan);
    Task<bool> DeleteMaintenancePlanAsync(int id);
    
    // NEW: Filter active plans
    Task<IEnumerable<MaintenancePlanDto>> GetActiveMaintenancePlansByCustomerIdAsync(int customerId);
}
```

### Step 3b: Verify/Implement the Filtering Method

**File:** `TorettoMotors.BLL/Services/Implementations/MaintenancePlanService.cs`

Confirm this implementation exists (add it only if missing):

```csharp
public async Task<IEnumerable<MaintenancePlanDto>> GetActiveMaintenancePlansByCustomerIdAsync(int customerId)
{
    var plans = await _maintenancePlanRepository.GetByCustomerIdAsync(customerId);
    
    // Filter to only "Active" status
    return plans
        .Where(p => p.Status == "Active" && p.StartDate.Date <= DateTime.Today)
        .Select(MapToDto);
}
```

**What's happening:**
- Call the repository to get all plans for the customer
- Filter to only those with `Status == "Active"` and `StartDate` is today or in the past
- Map to DTOs and return

---

## Step 4 — Verify Renewal Logic (~5 min)

### What You're Doing

Maintenance plans have a concept of renewal. Confirm the service method marks a plan as `"Renewal Pending"` when renewed.

This teaches: **Service-to-service coordination**. Your new method will call the repository but also might call other services (like a notification service in a real system).

### Step 4a: Verify Renewal Method in Interface

**File:** `TorettoMotors.BLL/Services/Interfaces/IMaintenancePlanService.cs`

Confirm this method exists in the interface (add it only if missing):

```csharp
/// <summary>
/// Mark a maintenance plan as pending renewal (called on annual anniversary).
/// </summary>
Task<MaintenancePlanDto> RenewMaintenancePlanAsync(int planId);
```

### Step 4b: Verify/Implement Renewal Logic

**File:** `TorettoMotors.BLL/Services/Implementations/MaintenancePlanService.cs`

Confirm this implementation exists (add it only if missing):

```csharp
public async Task<MaintenancePlanDto> RenewMaintenancePlanAsync(int planId)
{
    var plan = await _maintenancePlanRepository.GetByIdAsync(planId);
    if (plan == null)
        throw new ArgumentException($"Plan with ID {planId} not found");

    // Set status to "Renewal Pending" to signal it's ready to renew
    plan.Status = "Renewal Pending";

    var updated = await _maintenancePlanRepository.UpdateAsync(plan);
    return MapToDto(updated);
}
```

---

## Step 5 — Prepare the API Layer for Testing (~5 min)

### What You're Doing

Before Swagger testing, make sure the controller exposes your new BLL methods.  
The API should stay thin (no business rules), but it still needs endpoints for new operations.

### Step 5a: Add Active-Plans Endpoint

**File:** `TorettoMotors.Api/Controllers/MaintenancePlansController.cs`

Add this action to `MaintenancePlansController`:

```csharp
[HttpGet("customer/{customerId}/active")]
public async Task<ActionResult<IEnumerable<MaintenancePlanDto>>> GetActiveByCustomerId(int customerId)
{
    var plans = await _maintenancePlanService.GetActiveMaintenancePlansByCustomerIdAsync(customerId);
    return Ok(plans);
}
```

### Step 5b: Fix/Align Renewal Endpoint

Replace the existing renewal action with this action (no request body required), so it matches `IMaintenancePlanService.RenewMaintenancePlanAsync(int planId)`:

```csharp
[HttpPost("{id}/renew")]
public async Task<ActionResult<MaintenancePlanDto>> Renew(int id)
{
    try
    {
        var renewed = await _maintenancePlanService.RenewMaintenancePlanAsync(id);
        return Ok(renewed);
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
```

If your controller currently has a `RenewRequest` class used only for this endpoint, remove it.

### Step 5c: Return 400 for Service Validation Errors

Update `Create` and `Update` actions so `ArgumentException` from BLL becomes `400 Bad Request` (instead of an unhandled 500).  
Keep the controller thin: catch/translate only, business rules remain in BLL.

Example pattern:

```csharp
[HttpPost]
public async Task<ActionResult<MaintenancePlanDto>> Create(MaintenancePlanDto plan)
{
    try
    {
        var created = await _maintenancePlanService.CreateMaintenancePlanAsync(plan);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
```

### Step 5d: Verify the Build

From the terminal in the `Lab-start` folder:

```bash
dotnet build
```

All three projects should compile successfully. The API is now ready to exercise the new BLL behavior via Swagger.

---

## Step 6 — Test Your Changes (~10 min)

### What You're Doing

Test the new validation and filtering logic via the Swagger API.

### Step 6a: Run the Application

```bash
dotnet run --project TorettoMotors.Api
```

Navigate to the Swagger URL shown in terminal output.

### Step 6b: Create a Valid Maintenance Plan

1. In Swagger, find the **MaintenancePlans** section
2. Click `POST /api/MaintenancePlans` → "Try it out"
3. Enter a valid payload:

```json
{
  "customerId": 1,
  "name": "Premium Oil Change Package",
  "description": "Quarterly oil changes and filter replacement",
  "monthlyPrice": 25.00,
  "startDate": "2030-05-06T00:00:00",
  "status": "Active"
}
```

4. Click "Execute" — should succeed and return 201 Created

### Step 6c: Test Validation (Negative Price)

1. Click `POST /api/MaintenancePlans` → "Try it out"
2. Enter an invalid payload (negative price):

```json
{
  "customerId": 1,
  "name": "Invalid Plan",
  "description": "Should fail",
  "monthlyPrice": -10.00,
  "startDate": "2030-05-06T00:00:00",
  "status": "Active"
}
```

3. Click "Execute" — should fail with 400 Bad Request and your validation message

### Step 6d: Test Filtering

1. Click `GET /api/MaintenancePlans/customer/{customerId}/active`
2. Notice that only active plans are returned
3. If endpoint is missing, go back to Step 5 and add it before continuing

### Step 6e: Reflection

- ✅ You added validation logic to the BLL
- ✅ You added filtering logic to the BLL
- ✅ You added renewal logic to the BLL
- ✅ The API layer is thin and updated only to expose/translate BLL behavior
- ✅ The DAL is untouched — you used existing repository methods

That's N-Tier isolation.

---

## Completion Checklist

- [ ] Added validation to `MaintenancePlanService.CreateMaintenancePlanAsync()` (price > 0, required fields, valid dates)
- [ ] Added validation to `MaintenancePlanService.UpdateMaintenancePlanAsync()` (same validations)
- [ ] Added `GetActiveMaintenancePlansByCustomerIdAsync()` method to interface and implementation
- [ ] Added `RenewMaintenancePlanAsync()` method to interface and implementation
- [ ] Fixed `MaintenancePlansController` renewal action to call `RenewMaintenancePlanAsync(int)` (not `RenewAsync`)
- [ ] Verified `dotnet build` succeeds in all three projects
- [ ] Tested invalid price creation — correctly rejected
- [ ] Tested valid plan creation with a future-dated plan — correctly accepted
- [ ] Added/updated controller actions needed for active filtering and renewal testing
- [ ] Ensured controller translates BLL validation errors to `400 Bad Request`
- [ ] Understood the teaching point: **business rules in BLL, thin API surface for exposure/testing**
- [ ] Debrief: What would change if status values needed to be validated against an enum in the DAL?

---

## Troubleshooting

| Problem | Solution |
|---|---|
| **"Build failed: Type 'MaintenancePlanService' does not implement interface member"** | You added a method to the interface but didn't implement it in the service. Add the method to `MaintenancePlanService` |
| **"Invalid operation: API project doesn't reference BLL"** | Check `TorettoMotors.Api/TorettoMotors.Api.csproj` — it should reference `TorettoMotors.BLL`. If BLL is missing, the project reference is broken |
| **"MaintenancePlanDto has no field X"** | You're trying to map an entity field that doesn't exist in the DTO. Add it to both `MaintenancePlanDto` and update the `MapToDto()` method |
| **"Repository method GetByCustomerIdAsync doesn't exist"** | Check `IMaintenancePlanRepository.cs` — the method signature should be there. If missing, add it to the interface and implement in the repository |
| **Swagger doesn't show my new endpoint** | Service methods do not auto-create endpoints. Add the controller action in Step 5, rebuild, and refresh Swagger |
| **Validation doesn't trigger when I POST invalid data** | Ensure your validation code is in `CreateMaintenancePlanAsync()` and `UpdateMaintenancePlanAsync()`. Test with Swagger by sending negative price or past date |
| **`IMaintenancePlanService` has no definition for `RenewAsync`** | In `MaintenancePlansController`, replace `RenewAsync(...)` call with `RenewMaintenancePlanAsync(id)` and remove the unused request-body type |

---

## Debrief Questions

Reflect on what you learned about N-Tier architecture:

1. **Enforced Boundaries:** How would adding this feature be different in Module 1's monolith? What would stop you from putting validation logic in the controller?

2. **Interface-Based DI:** You depend on `IMaintenancePlanRepository`, not a concrete class. How does this help the backend team refactor the data layer without recompiling the BLL?

3. **Coordination Cost:** Creating a single feature required editing the service interface, the service implementation, and possibly the repository. Why is this overhead worth it?

4. **Composability:** You called `GetByCustomerIdAsync()` (a repository method) inside your new filtering method. Could you call another service method instead? What would that enable?

5. **Isolation:** The API layer changed only to expose service methods and map errors. Why is that still considered "thin" compared to putting business rules in controllers?

6. **Testing:** How would you unit-test `GetActiveMaintenancePlansByCustomerIdAsync()` without touching the database? (Hint: mock the repository interface)
